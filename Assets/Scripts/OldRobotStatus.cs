using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;


namespace ConsoleApplication1
{
    public class OldRobotStatus
    {
        public Dictionary<string, Dictionary<string, float>> targets;
        public Dictionary<string, float> imu_datas;
        public List<string> lidar_datas; // Change this when lidar will be used in the robot

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
        public OldRobotStatus(Dictionary<string, Dictionary<string, float>> targets, Dictionary<string, float> imu_datas,
            List<string> lidar_datas)
        {
            this.targets = targets;
            this.imu_datas = imu_datas;
            this.lidar_datas = lidar_datas;
        }

        public OldRobotStatus(string jsonString)
        {
            try
            {
                var msg = JsonConvert.DeserializeObject<OldRobotStatus>(jsonString);
                targets = msg.targets;
                imu_datas = msg.imu_datas;
                lidar_datas = msg.lidar_datas;
            }
            catch (JsonReaderException e)
            {
                targets = new Dictionary<string, Dictionary<string, float>>();
                imu_datas = new Dictionary<string, float>();
                lidar_datas = new List<string>();
            }
        }

        public Dictionary<string, float> ToPosDict()
        {
            var posDict = new Dictionary<string, float>();
            foreach (var key in targets.Keys)
            {
                if (_motorKeys.Contains(key))
                {
                    posDict[key] = targets[key]["position"];
                }
            }
            return posDict;
        }


        /// <summary>
        /// This method convert the OldRobotData object into a json string.
        /// </summary>
        ///<returns>A json string representing the OldRobotData object.</returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static Dictionary<string, float> ListToDict(List<float> listToConvert)
        {
            var dictToReturn = new Dictionary<string, float>();
            for (var i = 0; i < listToConvert.Count; i++)
            {
                dictToReturn[_motorKeys[i]] = listToConvert[i];
            }

            return dictToReturn;
        }
    }
}

/*
{"id": 3, "parity": 1, "len": 1124, "message": "{\"targets\": {\"rAnkleRX\": {\"position\": 0, \"torque\": 0}, \"lAnkleRX\": {\"position\
": 0, \"torque\": 0}, \"rAnkleRZ\": {\"position\": 0, \"torque\": 0}, \"lAnkleRZ\": {\"position\": 0, \"torque\": 0}, \"rShoulderRY\": {\
"position\": 0, \"torque\": 0}, \"lShoulderBaseRY\": {\"position\": 0, \"torque\": 0}, \"rShoulderBaseRY\": {\"position\": 0, \"torque\":
0}, \"lShoulderRY\": {\"position\": 0, \"torque\": 0}, \"rShoulderRZ\": {\"position\": 0, \"torque\": 0}, \"lShoulderRZ\": {\"position\"
: 0, \"torque\": 0}, \"rKneeRX\": {\"position\": 0, \"torque\": 0}, \"lKneeRX\": {\"position\": 0, \"torque\": 0}, \"rHipRX\": {\"positio
n\": 0, \"torque\": 0}, \"lHipRX\": {\"position\": 0, \"torque\": 0}, \"rHipRY\": {\"position\": 0, \"torque\": 0}, \"lHipRY\": {\"positi
on\": 0, \"torque\": 0}, \"rHipRZ\": {\"position\": 0, \"torque\": 0}, \"lHipRZ\": {\"position\": 0, \"torque\": 0}, \"headRX\": {\"posit
ion\": 0, \"torque\": 0}, \"rElbowRX\": {\"position\": 0, \"torque\": 0}, \"lElbowRX\": {\"position\": 0, \"torque\": 0}, \"torsoRY\": {\
"position\": 0, \"torque\": 0}}, \"imu_datas\": {\"accelerationX\": 0, \"accelerationY\": 0, \"accelerationZ\": 0, \"rotationX\": 0, \"ro
tationY\": 0, \"rotationZ\": 0, \"MagnX\": 0, \"MagnY\": 0, \"MagnZ\": 0}, \"lidar_datas\": []}"}

*/


// TEST
// var motorKeys = new List<string>
// {
// "rAnkleRX", "lAnkleRX", "rAnkleRZ", "lAnkleRZ",
// "rShoulderRY",
// "lShoulderBaseRY",
// "rShoulderBaseRY", "lShoulderRY", "rShoulderRZ", "lShoulderRZ", "rKneeRX",
// "lKneeRX",
// "rHipRX", "lHipRX", "rHipRY", "lHipRY", "rHipRZ", "lHipRZ", "headRX", "rElbowRX",
// "lElbowRX",
// "torsoRY"
// };
//
// var targets = new Dictionary<string, Dictionary<string, int>>();
//
// var pos = 30;
// var torque = 100;
// foreach (var key in motorKeys)
// {
// targets[key] = new Dictionary<string, int>
// {{"position", pos}, {"torque", torque}};
// }
//
// var imu = new Dictionary<string, int>
// {
// {"accelerationX", 0}, {"accelerationY", 0}, {"accelerationZ", 0}, {"rotationX", 0}, {"rotationY", 0},
// {"rotationZ", 0}, {"MagnX", 0}, {"MagnY", 0}, {"MagnZ", 0}
// };
//
// var lidar = new List<string>();
//
//
// var testPosition = new OldRobotStatus(targets, imu, lidar);
//
// UpdatePos(ConvertRealPosToSimulationPos(testPosition.ToPosDict()));

// FIN TEST