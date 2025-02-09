﻿using libMatrix.Backends;
using libMatrix.Helpers;
using libMatrix.Requests.Presence;
using libMatrix.Requests.Pushers;
using libMatrix.Requests.Rooms;
using libMatrix.Requests.Rooms.Message;
using libMatrix.Requests.UserData;
using MimeTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace libMatrix
{
    public partial class MatrixAPI
    {
        public const string VERSION = "r0.0.1";
        IMatrixAPIBackend _backend = null;
        private Events _events = null;

        private MatrixAppInfo _appInfo = null;

        public string UserID { get; private set; }
        public string DeviceID { get; private set; }
        public string DeviceName { get; private set; } = "libMatrix";
        //public string HomeServer { get; private set; }

        public string SyncToken { get; private set; } = "";
        public int SyncTimeout = 10000;

        public bool RunningInitialSync { get; private set; }
        public bool IsConnected { get; private set; }
        public Events Events { get => _events; set => _events = value; }
        public MatrixAppInfo AppInfo { get => _appInfo; }

        public MatrixAPI(string Url, string accessToken = "", string syncToken = "")
        {
            if (!Uri.IsWellFormedUriString(Url, UriKind.Absolute))
                throw new MatrixException("URL is not valid.");

            _backend = new HttpBackend(Url);
            _events = new Events();
            _appInfo = new MatrixAppInfo();

            if (!string.IsNullOrEmpty(accessToken))
                _backend.SetAccessToken(accessToken);

            SyncToken = syncToken;
            if (string.IsNullOrEmpty(SyncToken))
                RunningInitialSync = true;
        }

        private void FlushMessageQueue()
        {
            //throw new NotImplementedException();
        }

        public void SetUserID(string _userId)
        {
            UserID = _userId;
        }

        public void SetDeviceID(string _deviceId)
        {
            DeviceID = _deviceId;
        }

        public void SetDeviceName(string _deviceName)
        {
            DeviceName = _deviceName;
        }

        public async Task ClientSync(bool connectionFailureTimeout = false, bool fullState = false)
        {
            string url = "/_matrix/client/r0/sync?timeout=" + SyncTimeout;
            if (!string.IsNullOrEmpty(SyncToken))
                url += "&since=" + SyncToken;
            if (fullState)
                url += "&full_state=true";

            var tuple = await _backend.Get(url, true);
            MatrixRequestError err = tuple.Item1;
            string response = tuple.Item2;
            if (err.IsOk)
            {
                await ParseClientSync(response);
            }
            else if (connectionFailureTimeout)
            {

            }

            if (RunningInitialSync)
            {
                // Fire an event to say sync has been done

                RunningInitialSync = false;
            }
        }

        [MatrixSpec("r0.0.1/client_server.html#get-matrix-client-versions")]
        public async Task<string[]> ClientVersions()
        {
            var tuple = await _backend.Get("/_matrix/client/versions", false);
            MatrixRequestError err = tuple.Item1;
            string result = tuple.Item2;
            if (err.IsOk)
            {
                // Parse the version request

                return ParseClientVersions(result);
            }
            else
            {
                throw new MatrixException("Failed to validate version.");
            }
        }

        [MatrixSpec("r0.0.1/client_server.html#post-matrix-client-r0-register")]
        public async void ClientRegister(Requests.Session.MatrixRegister registration)
        {
            var tuple = await _backend.Post("/_matrix/client/r0/register", false, JsonHelper.Serialize(registration));
            MatrixRequestError err = tuple.Item1;
            string result = tuple.Item2;
            if (err.IsOk)
            {
                // Parse registration response
            }
            else
                throw new MatrixException(err.ToString());
        }

        [MatrixSpec("r0.0.1/client_server.html#post-matrix-client-r0-login")]
        public async Task ClientLogin(Requests.Session.MatrixLogin login)
        {
            var tuple = await _backend.Post("/_matrix/client/r0/login", false, JsonHelper.Serialize(login));
            MatrixRequestError err = tuple.Item1;
            string result = tuple.Item2;
            if (err.IsOk)
            {
                // We logged in!
                ParseLoginResponse(result);
            }
            else
            {
                //throw new MatrixException(err.ToString());
                Events.FireLoginFailEvent(err.ToString());
            }
        }

        public async void ClientProfile(string userId)
        {
            var tuple = await _backend.Get("/_matrix/client/r0/profile/" + userId, true);
            MatrixRequestError err = tuple.Item1;
            string result = tuple.Item2;
            if (err.IsOk)
            {
                var profileResponse = ParseUserProfile(result);

                Events.FireUserProfileReceivedEvent(userId, profileResponse.AvatarUrl, profileResponse.DisplayName);
            }
            else
            {
                // Fire an error
            }
        }

        public async Task<bool> ClientSetDisplayName(string displayName)
        {
            var req = new UserProfileSetDisplayName { DisplayName = displayName };
            var tuple = await _backend.Put(string.Format("/_matrix/client/r0/profile/{0}/displayname", Uri.EscapeDataString(UserID)), true, JsonHelper.Serialize(req));
            MatrixRequestError err = tuple.Item1;
            string result = tuple.Item2;
            if (err.IsOk)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> ClientSetAvatar(string avatarUrl)
        {
            var req = new UserProfileSetAvatar { AvatarUrl = avatarUrl };
            var tuple = await _backend.Put(string.Format("/_matrix/client/r0/profile/{0}/displayname", Uri.EscapeDataString(UserID)), true, JsonHelper.Serialize(req));
            MatrixRequestError err = tuple.Item1;
            string result = tuple.Item2;
            if (err.IsOk)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> ClientSetPresence(string presence, string statusMessage = null)
        {
            var req = new MatrixSetPresence
            {
                Presence = presence
            };

            if (statusMessage != null)
            {
                req.StatusMessage = statusMessage;
            }

            var tuple = await _backend.Put(string.Format("/_matrix/client/r0/presence/{0}/status", Uri.EscapeDataString(UserID)), true, JsonHelper.Serialize(req));
            MatrixRequestError err = tuple.Item1;
            string result = tuple.Item2;
            if (err.IsOk)
            {
                return true;
            }
            return false;
        }

        private async Task<string> UploadMedia(byte[] data, string fileName = null, string contentType = null)
        {
            if (contentType == null && fileName != null)
            {
                var fileExt = Path.GetExtension(fileName);
                contentType = MimeTypeMap.GetMimeType(fileExt);
            }
            if (contentType == null)
            {
                contentType = "application/octet-stream";
            }

            var meta = new Dictionary<string, string> { { "Content-Type", contentType } };
            var tuple = await _backend.Post("/_matrix/media/r0/upload", true, data, meta);
            MatrixRequestError err = tuple.Item1;
            string result = tuple.Item2;
            if (err.IsOk)
            {
                return ParseMediaUpload(result);
            }

            return null;
        }

        public string GetMediaDownloadUri(string contentUrl)
        {
            if (!contentUrl.StartsWith("mxc://"))
                return string.Empty;

            var newUrl = contentUrl.Remove(0, 6);
            var contentUrlSplit = newUrl.Split('/');
            if (contentUrlSplit.Count() < 2)
                return string.Empty;

            string uriPath = _backend.GetPath(string.Format("/_matrix/media/r0/download/{0}/{1}", contentUrlSplit[0], contentUrlSplit[1]), false);
            return uriPath;
        }

        public async Task<IEnumerable<string>> JoinedRooms()
        {
            var tuple = await _backend.Get("/_matrix/client/r0/joined_rooms", true);
            MatrixRequestError err = tuple.Item1;
            string result = tuple.Item2;
            if (err.IsOk)
            {
                // Parse joined rooms
                return ParseJoinedRooms(result);
            }
            else {
                throw new MatrixException(err.ToString());
            }
        }

        public async Task<bool> InviteToRoom(string roomId, string userId)
        {
			var invite = new MatrixRoomInvite { UserID = userId };
            var tuple = await _backend.Post(string.Format("/_matrix/client/r0/rooms/{0}/invite", Uri.EscapeDataString(roomId)), true, JsonHelper.Serialize(invite));
            MatrixRequestError err = tuple.Item1;
            string result = tuple.Item2;
            if (err.IsOk)
            {
                return true;
            }

            throw new MatrixException(err.ToString());
        }

        public async Task<string> ResolveRoomAlias(string roomAlias)
        {
            if (!roomAlias.StartsWith("#"))
                roomAlias = '#' + roomAlias;

            var tuple = await _backend.Get(string.Format("/_matrix/client/r0/directory/room/{0}", Uri.EscapeDataString(roomAlias)), true);
            MatrixRequestError err = tuple.Item1;
            string result = tuple.Item2;
            if (err.IsOk)
            {
                return ParseRoomAlias(result).RoomID;
            }

            return null;
        }

        public async Task<bool> JoinRoom(string roomId)
        {
			var roomJoin = new MatrixRoomJoin();
            var tuple = await _backend.Post(string.Format("/_matrix/client/r0/rooms/{0}/join", Uri.EscapeDataString(roomId)), true, JsonHelper.Serialize(roomJoin));
            MatrixRequestError err = tuple.Item1;
            string result = tuple.Item2;
            if (err.IsOk)
            {
                return true;
            }

            return false;
        }

        public async Task<string> CreateRoom(string roomName, string roomTopic, bool isDirect = false, 
            string[] invite = null, bool isPublic = true, string roomAlias = null)
        {
            if (string.IsNullOrWhiteSpace(roomName))
                return null;

            var roomCreate = new MatrixRoomCreate
			{
                Name = roomName,
                IsDirect = isDirect,
                InviteList = (invite ?? new string[0]).ToList(),
                Topic = roomTopic,
                Visibility = isPublic ? "public" : "private",
                RoomAliasName = roomAlias
            };

            var tuple = await _backend.Post("/_matrix/client/r0/createRoom", true, JsonHelper.Serialize(roomCreate));
            MatrixRequestError err = tuple.Item1;
            string result = tuple.Item2;
            if (err.IsOk)
            {
                try
                {
                    return ParseCreatedRoom(result).RoomID;
                }
                catch (MatrixException)
                {
                    return null;
                }
            }

            return null;
        }

        public async Task<bool> AddRoomAlias(string roomId, string alias)
        {
			var roomAddAlias = new MatrixRoomAddAlias
			{
                RoomID = roomId
            };

            var tuple = await _backend.Put(string.Format("/_matrix/client/r0/directory/room/{0}", Uri.EscapeDataString(alias)), true, JsonHelper.Serialize(roomAddAlias));
            MatrixRequestError err = tuple.Item1;
            string result = tuple.Item2;
            if (err.IsOk)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> DeleteRoomAlias(string roomAlias)
        {
            var tuple = await _backend.Delete(string.Format("/_matrix/client/r0/directory/room/{0}", Uri.EscapeDataString(roomAlias)), true);
            MatrixRequestError err = tuple.Item1;
            if (err.IsOk)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> LeaveRoom(string roomId)
        {
            var tuple = await _backend.Post(string.Format("/_matrix/client/r0/rooms/{0}/leave", Uri.EscapeDataString(roomId)), true, "");
            MatrixRequestError err = tuple.Item1;
            string result = tuple.Item2;
            if (err.IsOk)
            {
                return true;
            }

            return false;
        }

        public async void RoomTypingSend(string roomId, bool typing, int timeout = 0)
        {
			var req = new MatrixRoomSendTyping { Typing = typing };
            if (timeout > 0)
                req.Timeout = timeout;

            var tuple = await _backend.Put(string.Format("/_matrix/client/r0/rooms/{0}/typing/{1}", Uri.EscapeDataString(roomId), Uri.EscapeDataString(UserID)), true, JsonHelper.Serialize(req));
            MatrixRequestError err = tuple.Item1;
            string result = tuple.Item2;
            if (!err.IsOk)
                throw new MatrixException(err.ToString());
        }

        public async Task<bool> GetRoomState(string roomId, string eventType = null, string stateKey = null)
        {
            string url = string.Format("/_matrix/client/r0/rooms/{0}/state", roomId);
            if (!string.IsNullOrEmpty(eventType))
                url += "/" + eventType;
            if (!string.IsNullOrEmpty(stateKey))
                url += "/" + stateKey;

            var tuple = await _backend.Get(url, true);
            MatrixRequestError err = tuple.Item1;
            string result = tuple.Item2;
            if (err.IsOk)
            {
                // Parse stuff
                // Parsing will differ if there is no eventType specified

                if (!string.IsNullOrEmpty(eventType))
                {

                }
                else
                {

                }

                return true;
            }

            return false;
        }

        private async Task<bool> SendEventToRoom(string roomId, string eventType, string content)
        {
            var tuple = await _backend.Put(string.Format("/_matrix/client/r0/rooms/{0}/send/{1}/{2}", Uri.EscapeDataString(roomId), eventType, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()), true, content);
            MatrixRequestError err = tuple.Item1;
            string result = tuple.Item2;
            if (err.IsOk)
                return true;

            return false;
        }

        public async Task<bool> SendTextMessageToRoom(string roomId, string message, TextMessageKind kind = default)
        {
            ITextMessage req;
            switch (kind)
            {
                case TextMessageKind.Text: req = new MatrixRoomMessageText(); break;
                case TextMessageKind.Notice: req = new MatrixRoomMessageNotice(); break;
                case TextMessageKind.Emote: req = new MatrixRoomMessageEmote(); break;
                default: throw new InvalidOperationException(kind.ToString());
            }
            req.Body = message;

            return await SendEventToRoom(roomId, "m.room.message", JsonHelper.Serialize(req));
        }

        public async Task<bool> SendFileMessageToRoom(string roomId, string fileName, byte[] bytes, FileMessageKind kind = default)
        {
            IFileMessage req;
            switch (kind)
            {
                case FileMessageKind.Image: req = new MatrixRoomMessageImage(); break;
                case FileMessageKind.Audio: req = new MatrixRoomMessageAudio(); break;
                case FileMessageKind.File: req = new MatrixRoomMessageFile(); break;
                case FileMessageKind.Video: req = new MatrixRoomMessageVideo(); break;
                default: throw new InvalidOperationException(kind.ToString());
            }
            req.Description = fileName;
            req.MediaUrl = await UploadMedia(bytes, fileName);

            return await SendEventToRoom(roomId, "m.room.message", JsonHelper.Serialize(req));
        }

        public async Task<bool> SendLocationToRoom(string roomId, string description, double lat, double lon)
        {
            var sb = new StringBuilder("geo:");
            sb.Append(lat);
            sb.Append(",");
            sb.Append(lon);
            var req = new MatrixRoomMessageLocation
            {
                Description = description,
                GeoUri = sb.ToString()
            };

            return await SendEventToRoom(roomId, "m.room.message", JsonHelper.Serialize(req));
        }

        public async Task<bool> SetPusher(string pushUrl, string pushKey)
        {
            var req = new MatrixSetPusher
            {
                PushKey = pushKey,
                Kind = "http",
                AppID = AppInfo.ApplicationID,
                AppDisplayName = AppInfo.ApplicationName,
                DeviceDisplayName = DeviceName,
                Language = "en",
                Append = false
            };
            req.Data = new MatrixPusherData
			{
                Url = pushUrl,
                Format = "event_id_only"
            };

            var jsonData = JsonHelper.Serialize(req);

            var tuple = await _backend.Post("/_matrix/client/r0/pushers/set", true, jsonData);
            MatrixRequestError err = tuple.Item1;
            string result = tuple.Item2;
            if (err.IsOk)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> GetNotifications(string from = "", int limit = -1, string only = "")
        {
            var url = new StringBuilder("/_matrix/client/r0/notifications");

            var urlParams = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(from))
                urlParams.Add("from", from);
            if (limit != -1)
                urlParams.Add("limit", limit.ToString());
            if (!string.IsNullOrEmpty(only))
                urlParams.Add("only", only);

            if (urlParams.Count > 0)
            {
                var enc = new FormUrlEncodedContent(urlParams);
                url.Append("?" + enc.ReadAsStringAsync().Result);
            }

            var tuple = await _backend.Get(url.ToString(), true);
            MatrixRequestError err = tuple.Item1;
            string result = tuple.Item2;
            if (err.IsOk)
            {
                // Parse the response
                ParseNotifications(result);
                return true;
            }
            return false;
        }
    }
}
