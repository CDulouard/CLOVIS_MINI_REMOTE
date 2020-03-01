using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ConsoleApplication1
{
    public class OldRobotData
    {
        public Dictionary<string, Dictionary<string, int>> targets;

        private static List<string> _motorKeys = new List<string>
        {
            "rAnkleRX", "lAnkleRX", "rAnkleRZ", "lAnkleRZ",
            "rShoulderRY",
            "lShoulderBaseRY",
            "rShoulderBaseRY", "lShoulderRY", "rShoulderRZ", "lShoulderRZ", "rKneeRX",
            "lKneeRX",
            "rHipRX", "lHipRX", "rHipRY", "lHipRY", "rHipRZ", "lHipRZ", "headRX", "rElbowRX",
            "lElbowRX",
            "torsoRY"
        };

        [JsonConstructor]
        public OldRobotData(Dictionary<string, Dictionary<string, int>> targets)
        {
            this.targets = targets;
        }

        public OldRobotData(string jsonString)
        {
            try
            {
                var msg = JsonConvert.DeserializeObject<OldRobotData>(jsonString);
                targets = msg.targets;
            }
            catch (JsonReaderException e)
            {
                targets = new Dictionary<string, Dictionary<string, int>>();
            }
        }

        public OldRobotData(Dictionary<string, int> positions = null, Dictionary<string, int> torques = null)
        {
            if (torques != null)
            {
                if (_motorKeys.Any(key => !torques.ContainsKey(key)))
                {
                    torques = null;
                }
            }

            if (torques == null)
            {
                torques = new Dictionary<string, int>();
                foreach (var key in _motorKeys)
                {
                    torques[key] = 100;
                }
            }

            if (positions != null)
            {
                if (_motorKeys.Any(key => !positions.ContainsKey(key)))
                {
                    positions = null;
                }
            }

            if (positions == null)
            {
                positions = new Dictionary<string, int>();
                foreach (var key in _motorKeys)
                {
                    positions[key] = 1;
                }
            }

            targets = new Dictionary<string, Dictionary<string, int>>();
            foreach (var key in _motorKeys)
            {
                targets[key] = new Dictionary<string, int>
                    {{"position", positions[key]}, {"torque", torques[key]}};
            }
        }

        /// <summary>
        /// This method convert the OldRobotData object into a json string.
        /// </summary>
        ///<returns>A json string representing the OldRobotData object.</returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static Dictionary<string, int> ListToDict(List<int> listToConvert)
        {
            var dictToReturn = new Dictionary<string, int>();
            for (var i = 0; i < listToConvert.Count; i++)
            {
                dictToReturn[_motorKeys[i]] = listToConvert[i];
            }

            return dictToReturn;
        }
    }
}