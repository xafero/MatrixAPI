using libMatrix.Responses;
using libMatrix.Responses.Events;
using libMatrix.Responses.Events.Room;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace libMatrix.Helpers
{
	public class MatrixEventsItemConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(MatrixEvents).IsAssignableFrom(objectType);
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var lst = new List<MatrixEvents>();

            if (reader.TokenType == JsonToken.StartArray)
            {
                JArray jsonArray = JArray.Load(reader);

                foreach (var item in jsonArray)
                {
                    switch (item["type"].Value<string>())
                    {
                        case "m.direct":
                            lst.Add(item.ToObject<Direct>());
                            break;

                        case "m.presence":
                            lst.Add(item.ToObject<Presence>());
                            break;

                        case "m.typing":
                            lst.Add(item.ToObject<Typing>());
                            break;

                        case "m.room.avatar":
                            lst.Add(item.ToObject<Avatar>());
                            break;

                        case "m.room.canonical_alias":
                            lst.Add(item.ToObject<CanonicalAlias>());
                            break;

                        case "m.room.create":
                            lst.Add(item.ToObject<Create>());
                            break;

                        case "m.room.guest_access":
                            lst.Add(item.ToObject<GuestAccess>());
                            break;

                        case "m.room.join_rules":
                            lst.Add(item.ToObject<JoinRules>());
                            break;

                        case "m.room.member":
                            lst.Add(item.ToObject<Member>());
                            break;

                        case "m.room.message":
                            lst.Add(item.ToObject<Message>());
                            break;

                        case "m.room.name":
                            lst.Add(item.ToObject<Name> ());
                            break;

                        case "m.room.topic":
                            lst.Add(item.ToObject<Topic>());
                            break;

                        case "m.sticker":
                            lst.Add(item.ToObject<Sticker>());
                            break;

                        default:
                            Debug.WriteLine("Unknown event type: " + item["type"]);
                            lst.Add(item.ToObject<MatrixEvents>());
                            break;
                    }

                }
            }


            return lst;
            //return item.ToObject<MatrixEvents>();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    public class MatrixEventsRoomMessageItemConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(MessageContent).IsAssignableFrom(objectType);
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {

            if (reader.TokenType == JsonToken.StartObject)
            {
                JObject item = JObject.Load(reader);

                switch (item["msgtype"].Value<string>())
                {
                    case "m.image":
                        return item.ToObject<MessageImageContent>();

                    case "m.location":
                        return item.ToObject<MessageLocationContent>();

                    case "m.text":
                    case "m.notice":
                    case "m.emote":
                        return item.ToObject<MessageContent>();

                    default:
                        Debug.WriteLine("Unknown message type: " + item["msgtype"]);
                        return item.ToObject<MessageContent>();
                }
            }

            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
