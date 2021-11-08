using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using libMatrix.Responses;
using libMatrix.Responses.Media;
using libMatrix.Responses.Pushers;
using libMatrix.Responses.Rooms;
using libMatrix.Responses.Session;
using libMatrix.Responses.UserData;
using Newtonsoft.Json;

namespace libMatrix
{
	public partial class MatrixAPI
    {
        private string[] ParseClientVersions(string resp)
        {
            var response = ParseResponse<VersionResponse>(resp);
            return response.Versions;
        }

        private void ParseLoginResponse(string resp)
        {
            var response = ParseResponse<LoginResponse>(resp);

            UserID = response.UserID;
            DeviceID = response.DeviceID;

            _backend.SetAccessToken(response.AccessToken);

            _events.FireLoginEvent(response);
        }

        private static T ParseResponse<T>(string resp) where T : class
        {
            var type = typeof(T);
            try
            {
                var bytes = Encoding.UTF8.GetBytes(resp);
                using (var stream = new MemoryStream(bytes))
                {
                    var ser = new DataContractJsonSerializer(type);
                    var obj = ser.ReadObject(stream);
                    var response = obj as T;
                    return response;
                }
            }
            catch
            {
                throw new MatrixException($"Failed to parse {type.Name}");
            }
        }

        private UserProfileResponse ParseUserProfile(string resp)
        {
            var response = ParseResponse<UserProfileResponse>(resp);
            return response;
        }

        private async Task ParseClientSync(string resp)
        {
            try
            {
                    var response = JsonConvert.DeserializeObject<MatrixSync>(resp);

                    SyncToken = response.NextBatch;

                    foreach (var room in response.Rooms.Join)
                    {
                        _events.FireRoomJoinEvent(room.Key, room.Value);
                    }

                    foreach (var room in response.Rooms.Invite)
                    {
                        _events.FireRoomInviteEvent(room.Key, room.Value);
                    }

                    foreach (var room in response.Rooms.Leave)
                    {
                        _events.FireRoomLeaveEvent(room.Key, room.Value);
                    }

                    if (response.Presense != null)
                    {
                        foreach (var evt in response.Presense.Events)
                        {
                            var actualEvent = evt as Responses.Events.Presence;
                            bool active = actualEvent.Content.CurrentlyActive;
                        }
                    }

                    if (response.AccountData != null)
                    {
                        foreach (var evt in response.AccountData.Events)
                        {
                            Debug.WriteLine("AccountData Event: " + evt.Type);
                            _events.FireAccountDataEvent(evt);
                        }
                    }

                    IsConnected = true;
            }
            catch (Exception e)
            {
                throw new MatrixException("Failed to parse ClientSync - " + e.Message);
            }
        }

        private string ParseMediaUpload(string resp)
        {
            var response = ParseResponse<MediaUploadResponse>(resp);
            return response.ContentUri;
        }

        private List<string> ParseJoinedRooms(string resp)
        {
            var response = ParseResponse<JoinedRooms>(resp);
            var thing = response.Rooms;
            return thing;
        }

        private CreateRoom ParseCreatedRoom(string resp)
        {
            var response = ParseResponse<CreateRoom>(resp);
            Events.FireRoomCreateEvent(response.RoomID);
            return response;
        }

        private RoomAlias ParseRoomAlias(string resp)
        {
            var response = ParseResponse<RoomAlias>(resp);
            Events.FireRoomAliasEvent(response.RoomID, response.Servers);
            return response;
        }

        private void ParseNotifications(string resp)
        {
            var response = ParseResponse<MatrixNotifications>(resp);

            Console.WriteLine("Notifications received.");

            foreach (var notification in response.Notifications)
            {
                Events.FireNotificationEvent(notification);
            }
        }
    }
}
