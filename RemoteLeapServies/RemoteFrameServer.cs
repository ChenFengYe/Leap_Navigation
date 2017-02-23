using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Leap;
using Leap.Unity;
using System.Collections.Generic;
using System.Timers;
using LitJson;
using Newtonsoft.Json;

public class RemoteFrameServer : MonoBehaviour
{
    public LeapServiceProvider  LocalLeapService;
    //public WebSocket            w = new WebSocket(new Uri("ws://localhost:6437/v7.json"));
    public WebSocket            w = new WebSocket(new Uri("ws://10.19.127.228:6437/v7.json"));
    public Frame                RemoteFrame;
    private bool                IsDataInQuaue;
    private int                 FingersNumbers = 5;
    //private System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
    IEnumerator Start()
    {
        RemoteFrame = new Frame();
        IsDataInQuaue = false;

        yield return StartCoroutine(w.Connect());

        // Send background command
        w.SendString("{\"background\":True}");
    }

    void Update()
    {
        RemoteFrame = GetRemoteFrame();
        //if (IsDataInQuaue == true)
        //    Debug.Log("Remote Hands Numbers: " + RemoteFrame.Hands.Count);

        //timer.Reset();
        //timer.Start();
        //timer.Stop();
        //Debug.Log(timer.ElapsedMilliseconds.ToString());
        //// 可以使用计时器来进行测试 延时时间 即 远程手部数据刷新帧率
    }

    public void Close()
    {
        w.Close();
    }

    public FrameData GetJSON(out string reply)
    {
        // Send focused command
        w.SendString("{\"focused\": true}");

        // Receive data
        reply = w.RecvString();

        // Check Is Data in Quaue
        if (reply == null)
        {
            IsDataInQuaue = false;
            return null;
        }
        else
            IsDataInQuaue = true;

        //// Method 1
        // Get Object from JSON Data
        //FrameData frame = JsonUtility.FromJson<FrameData>(reply);
        
        //// Method 2
        // format set
        //JsonMapper.RegisterImporter<double, float>(input => Convert.ToSingle(input));
        //JsonMapper.RegisterImporter<double, int>(input => Convert.ToInt32(input));
        //LitJson.JsonReader JSONData = new LitJson.JsonReader(reply);
        //JSONData.SkipNonMembers = true;
        //FrameData frame = JsonMapper.ToObject<FrameData>(JSONData);

        // Method 3
        FrameData frame = JsonConvert.DeserializeObject<FrameData>(reply);

        return frame;
    }

    public Frame GetRemoteFrame()
    {
        string reply;
        FrameData Jframe = GetJSON(out reply);

        // Ckeck there exists FrameData or not, if not Don't Change RemoteHands
        if (Jframe == null)
            return RemoteFrame;
        Frame frame = new Frame
            (Jframe.id,
            Jframe.timestamp,
            Jframe.currentFrameRate,
            GetRemoteInteractionBox(Jframe),
            GetRemoteHand(Jframe));
        Frame transforedframe = new Frame();
        LocalLeapService.transformFrame(frame, transforedframe);

        return transforedframe;
    }

    public List<Hand> GetRemoteHand(FrameData Jframe)
    {
        List<Hand> hands = new List<Hand>();

        for (int i = 0; i < Jframe.hands.Count; i++)
        {
            Hands hand = Jframe.hands[i];
            //----------------------------------------------------------------------------------------------------------
            // Arm
            Vector elbow = toVec3(hand.elbow);
            Vector wrist = toVec3(hand.wrist);
            Arm arm = new Arm
                (elbow,
                wrist,
                (elbow + wrist) * 0.5f,
                (wrist - elbow).Normalized,
                wrist.DistanceTo(elbow),
                hand.armWidth,
                CreateLeapQuater(hand.armBasis, hand.type));

            //----------------------------------------------------------------------------------------------------------
            // Fingers
            List<Finger> fingers = new List<Finger>();

            if (Jframe.pointables == null || Jframe.pointables.Count == 0)
            {
                Debug.Log("JSON　Data Error: The Pointables have nothing from hands building!");
                return hands;
            }

            Pointables pointable = new Pointables();
            for (int j = 0; j < FingersNumbers; j++)
            {
                for (int i_finger = 0; i_finger < Jframe.pointables.Count; i_finger++)
                {
                    pointable = Jframe.pointables[i_finger];
                    if (pointable.handId == hand.id && pointable.type == j)
                    {
                        break;
                    }
                }

                if (pointable.handId != hand.id || pointable.type != j)
                {
                    Console.ReadKey();                
                }
                Bone metacarpal     = CreateBone(hand.type, pointable, pointable.bases[0], toVec3(pointable.carpPosition), toVec3(pointable.mcpPosition), Bone.BoneType.TYPE_METACARPAL);
                Bone proximal       = CreateBone(hand.type, pointable, pointable.bases[1], toVec3(pointable.mcpPosition), toVec3(pointable.pipPosition), Bone.BoneType.TYPE_PROXIMAL);
                Bone intermediate   = CreateBone(hand.type, pointable, pointable.bases[2], toVec3(pointable.pipPosition), toVec3(pointable.dipPosition), Bone.BoneType.TYPE_INTERMEDIATE);
                Bone distal         = CreateBone(hand.type, pointable, pointable.bases[3], toVec3(pointable.dipPosition), toVec3(pointable.btipPosition), Bone.BoneType.TYPE_DISTAL);
                Finger f = new Finger
                    (Jframe.id,
                    pointable.handId,
                    pointable.id,
                    pointable.timeVisible,
                    toVec3(pointable.tipPosition),
                    toVec3(pointable.tipVelocity),
                    toVec3(pointable.direction),
                    toVec3(pointable.stabilizedTipPosition),
                    pointable.width,
                    pointable.length,
                    pointable.extended,
                    (Finger.FingerType)pointable.type,
                    metacarpal,     // metacarpal - The first bone of the finger (inside the hand).
                    proximal,       // proximal - The second bone of the finger
                    intermediate,   // intermediate - The third bone of the finger.
                    distal);        // distal - The end bone.

                fingers.Add(f);
            }

            //----------------------------------------------------------------------------------------------------------
            // Hands
            Hand RemoteHand = new Hand
                (Jframe.id,
                hand.id,
                hand.confidence,
                hand.grabStrength,
                hand.grabAngle,
                hand.pinchStrength,
                hand.pinchDistance,
                hand.palmWidth,
                (hand.type == "left") ? true : false,
                hand.timeVisible,
                arm,// Arm
                fingers,// Finger List
                toVec3(hand.palmPosition),
                toVec3(hand.stabilizedPalmPosition),
                toVec3(hand.palmVelocity),
                toVec3(hand.palmNormal),
                toVec3(hand.direction),
                toVec3(hand.wrist));
            hands.Add(RemoteHand);
        }
        return hands;
    }

    public Bone CreateBone(string handType, Pointables finger, List<List<float>> basis, Vector PreJoint, Vector NextJoint, Bone.BoneType type)
    {

        // Build Bine
        Bone bone = new Bone
            (PreJoint,                          // prevJoint - The proximal end of the bone (closest to the body)
            NextJoint,                          // nextJoint - The distal end of the bone (furthest from the body)
            (PreJoint + NextJoint) * 0.5f,      // center - The midpoint of the bone
            PreJoint + NextJoint,               // direction - The unit direction vector pointing from prevJoint to nextJoint.
            PreJoint.DistanceTo(NextJoint),     // length - The estimated length of the bone.
            finger.width,                       // width - The estimated average width of the bone.
            type,                               // type - The type of finger bone.
            CreateLeapQuater(basis, handType));           // basis - The matrix representing the orientation of the bone.
        return bone;
    }

    public LeapQuaternion CreateLeapQuater(List<List<float>> basis, string handType)
    {
        // Build a Rotation matrix……
        Matrix4x4 rotation = new Matrix4x4();
        if (handType == "right")
        {
            // x-axsis
            rotation[0, 0] = basis[0][0];
            rotation[1, 0] = basis[0][1];
            rotation[2, 0] = basis[0][2];
            // y-axsis
            rotation[0, 1] = basis[1][0];
            rotation[1, 1] = basis[1][1];
            rotation[2, 1] = basis[1][2];
            // z-axsis
            rotation[0, 2] = basis[2][0];
            rotation[1, 2] = basis[2][1];
            rotation[2, 2] = basis[2][2];

            Quaternion q = rotation.GetRotation();
            LeapQuaternion Leapq = new LeapQuaternion(q.x, q.y, q.z, q.w);
            return Leapq;
        }
        else
        {
            // x-axsis
            rotation[0, 0] = basis[0][0];
            rotation[1, 0] = basis[0][1];
            rotation[2, 0] = basis[0][2];
            // y-axsis
            rotation[0, 1] = basis[1][0];
            rotation[1, 1] = basis[1][1];
            rotation[2, 1] = basis[1][2];
            // z-axsis
            rotation[0, 2] = -basis[2][0];
            rotation[1, 2] = -basis[2][1];
            rotation[2, 2] = -basis[2][2];

            Quaternion q = rotation.GetRotation();
            LeapQuaternion Leapq = new LeapQuaternion(q.x, q.y, q.z, q.w);
            return Leapq;
        }

        
    }

        //    // Right Hand is right hand coordiate
        //if(handType == "right")
        //{
        //    rotation[0, 2] = -basis[2][0];
        //    rotation[1, 2] = -basis[2][1];
        //    rotation[2, 2] = -basis[2][2];
        //}
        //// Left Hand is Left hand coordiate  for Unity is Left Hand corrdiate
        //else
        //{
        //    rotation[0, 2] = basis[2][0];
        //    rotation[1, 2] = basis[2][1];
        //    rotation[2, 2] = basis[2][2];
        //}


    public Leap.InteractionBox GetRemoteInteractionBox(FrameData Jframe)
    {
        if (Jframe.interactionBox == null)
        {
            Leap.InteractionBox RemoteBox = new Leap.InteractionBox();
            return RemoteBox;
        }
        Leap.InteractionBox RemoteBox1 = new Leap.InteractionBox
            (toVec3(Jframe.interactionBox.center),
            toVec3(Jframe.interactionBox.size));
        return RemoteBox1;
    }

    public Vector toVec3(List<float> a)
    {
        Vector b = new Vector(a[0], a[1], a[2]);
        return b;
    }
    public Vector toVec3(List<int> a)
    {
        Vector b = new Vector(a[0], a[1], a[2]);
        return b;
    }
}

[Serializable]
public class Gestures
{
    /// Center
    public List<float> center;
     
    /// Direction
    public List<float> direction;
     
    /// Duration
    public int duration;
     
    /// HandIds
    public List<int> handIds;
     
    /// Id
    public int id;

    /// Normal
    public List<float> normal;
     
    /// PointableIds
    public List<int> pointableIds;
     
    /// Position
    public List<float> position;
     
    /// Progress
    public float progress;

    /// Radius
    public float radius;
     
    /// Speed
    public float speed;

    /// StartPosition
    public List<float> startPosition;
     
    /// stop
    public string state;
     
    /// keyTap
    public string type;
}

[Serializable]
public class Hands
{
    /// ArmBasis
    public List<List<float>> armBasis;

    /// ArmWidth
    public float armWidth;

    /// Confidence
    public float confidence;

    /// Direction
    public List<float> direction;

    /// Elbow
    public List<float> elbow;

    /// GrabAngle
    public float grabAngle;

    /// GrabStrength
    public float grabStrength;

    /// Id
    public int id;

    /// PalmNormal
    public List<float> palmNormal;

    /// PalmPosition
    public List<float> palmPosition;

    /// PalmVelocity
    public List<float> palmVelocity;

    /// PalmWidth
    public float palmWidth;

    /// PinchDistance
    public float pinchDistance;

    /// PinchStrength
    public float pinchStrength;

    /// R
    public List<List<float>> r;

    /// S
    public float s;

    /// SphereCenter
    public List<float> sphereCenter;

    /// SphereRadius
    public float sphereRadius;

    /// StabilizedPalmPosition
    public List<float> stabilizedPalmPosition;

    /// T
    public List<float> t;

    /// TimeVisible
    public float timeVisible;

    /// right
    public string type;

    /// Wrist
    public List<float> wrist;
}

[Serializable]
public class InteractionBox
{
    /// Center
    public List<float> center;

    /// Size
    public List<float> size;
}

[Serializable]
public class Pointables
{
    /// Bases
    public List<List<List<float>>> bases;

    /// BtipPosition
    public List<float> btipPosition;

    /// CarpPosition
    public List<float> carpPosition;

    /// DipPosition
    public List<float> dipPosition;

    /// Direction
    public List<float> direction;

    /// Extended
    public bool extended;

    /// HandId
    public int handId;

    /// Id
    public int id;

    /// Length
    public float length;

    /// McpPosition
    public List<float> mcpPosition;

    /// PipPosition
    public List<float> pipPosition;

    /// StabilizedTipPosition
    public List<float> stabilizedTipPosition;

    /// TimeVisible
    public float timeVisible;

    /// TipPosition
    public List<float> tipPosition;

    /// TipVelocity
    public List<float> tipVelocity;

    /// Tool
    public bool tool;

    /// TouchDistance
    public float touchDistance;

    /// touching
    public string touchZone;

    /// Type
    public int type;

    /// Width
    public float width;
}

[Serializable]
public class FrameData
{
    /// CurrentFrameRate
    public float currentFrameRate;

    /// Devices
    public List<string> devices;

    /// Gestures
    public List<Gestures> gestures;

    /// Hands
    public List<Hands> hands;

    /// Id
    public int id;

    /// InteractionBox
    public InteractionBox interactionBox;

    /// Pointables
    public List<Pointables> pointables;

    /// R
    public List<List<float>> r;

    /// S
    public float s;

    /// T
    public List<float> t;

    /// Timestamp
    public Int64 timestamp;
}
