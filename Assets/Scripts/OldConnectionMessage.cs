using Newtonsoft.Json;

namespace ConsoleApplication1
{
    public class OldConnectionMessage
    {
        public int port;
        public string pass;


        [JsonConstructor]
        public OldConnectionMessage(string pass, int port)
        {
            this.pass = pass;
            this.port = port;
        }

        public OldConnectionMessage(string pass, int port, bool hashPass = false)
        {
            if (hashPass)
            {
                pass = UdpSocket.CryptPass(pass);
            }

            this.pass = pass;
            this.port = port;
        }

        public OldConnectionMessage(string jsonString)
        {
            try
            {
                var msg = JsonConvert.DeserializeObject<OldConnectionMessage>(jsonString);
                pass = msg.pass;
                port = msg.port;
            }
            catch (JsonReaderException e)
            {
                pass = "";
                port = 0;
            }
        }

        /// <summary>
        /// This method convert the Message object into a json string.
        /// </summary>
        ///<returns>A json string representing the Message object.</returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}