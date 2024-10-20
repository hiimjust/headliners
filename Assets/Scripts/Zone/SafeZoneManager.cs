using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SafeZoneManager : MonoBehaviourPunCallbacks, IPunObservable
{
    private PhotonView PV;

    [Range(0, 360)]
    public int Segments;
    [Range(0, 5000)]
    public float XRadius;

    [Range(10, 100)]
    public int ZoneRadiusFactor = 50; 

    [Header("Shrinking Zones")]
    public List<int> ZoneTimes;

    #region Private Members
    public bool Shrinking;
    private int countdownPrecall = 10;
    private int timeToShrink = 30;
    private int count = 0;
    private bool newCenterObtained = false;
    private Vector3 centerPoint = new Vector3(0, -100, 0);
    private float distanceToMoveCenter;
    private WorldCircle circle;
    private LineRenderer renderer;
    private GameObject ZoneWall;
    private float shrinkRadius;
    private int zoneRadiusIndex = 0;
    private int zoneTimesIndex = 0;
    private float timePassed;
    #endregion

    public static byte ShrinkingCountdownEvent = 1;

    public static Vector3 CurrentCenter;
    public static float CurrentRadius;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        CurrentCenter = transform.position;
        CurrentRadius = XRadius;
    }

    void Start()
    {
        renderer = gameObject.GetComponent<LineRenderer>();
        circle = new WorldCircle(ref renderer, Segments, new float[] { XRadius, XRadius });
        ZoneWall = GameObject.FindGameObjectWithTag("ZoneWall");

        if (PhotonNetwork.IsMasterClient)
        {
            PV.RPC("InitializeSafeZone", RpcTarget.AllViaServer, transform.position, XRadius);
        }

        timePassed = Time.deltaTime;
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        ZoneWall.transform.localScale = new Vector3((XRadius * 0.01f), 1, (XRadius * 0.01f));

        if (Shrinking)
        {
            if (!newCenterObtained)
            {
                centerPoint = NewCenterPoint(transform.position, XRadius, shrinkRadius);
                distanceToMoveCenter = Vector3.Distance(transform.position, centerPoint);
                newCenterObtained = (centerPoint != new Vector3(0, -100, 0));
            }

            transform.position = Vector3.MoveTowards(transform.position, centerPoint, distanceToMoveCenter / timeToShrink * Time.deltaTime);
            ZoneWall.transform.position = transform.position;

            XRadius = Mathf.MoveTowards(XRadius, shrinkRadius, shrinkRadius / timeToShrink * Time.deltaTime);
            circle.Draw(Segments, XRadius, XRadius);

            if (1 > (XRadius - shrinkRadius))
            {
                timePassed = Time.deltaTime;
                Shrinking = false;
                newCenterObtained = false;
            }

            PV.RPC("UpdateSafeZone", RpcTarget.AllViaServer, transform.position, XRadius);
        }
        else
        {
            timePassed += Time.deltaTime; // increment clock time
        }

        if (zoneTimesIndex + 1 >= ZoneTimes.Count)
        {
            Shrinking = false;
        }
        else
        {
            if (((int)timePassed) > ZoneTimes[zoneTimesIndex])
            {
                shrinkRadius = ShrinkCircle((float)(XRadius * (ZoneRadiusFactor * 0.01)))[1]; 
                Shrinking = true;
                timePassed = Time.deltaTime;  
                NextZoneTime();
            }

            if (timePassed > (ZoneTimes[zoneTimesIndex] - countdownPrecall))
            {
                if (ZoneTimes[zoneTimesIndex] - (int)timePassed != count)
                {
                    count = Mathf.Clamp(ZoneTimes[zoneTimesIndex] - (int)timePassed, 1, 1000);

                    object[] content = new object[] { count };
                    RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
                    PhotonNetwork.RaiseEvent(ShrinkingCountdownEvent, content, raiseEventOptions, SendOptions.SendReliable);
                }
            }
        }
        CurrentCenter = transform.position;
        CurrentRadius = XRadius;
    }

    [PunRPC]
    private void InitializeSafeZone(Vector3 startPosition, float initialRadius)
    {
        transform.position = startPosition;
        XRadius = initialRadius;
        circle.Draw(Segments, XRadius, XRadius);
    }

    [PunRPC]
    private void UpdateSafeZone(Vector3 newPosition, float newRadius)
    {
        transform.position = newPosition;
        XRadius = newRadius;
        circle.Draw(Segments, XRadius, XRadius);
        ZoneWall.transform.position = newPosition;
        ZoneWall.transform.localScale = new Vector3((newRadius * 0.01f), 1, (newRadius * 0.01f));
    }

    private Vector3 NewCenterPoint(Vector3 currentCenter, float currentRadius, float newRadius)
    {
        Vector3 newPoint = Vector3.zero;

        var totalCountDown = 30000; 
        var foundSuitable = false;
        while (!foundSuitable)
        {
            totalCountDown--;
            Vector2 randPoint = Random.insideUnitCircle * (currentRadius * 2.0f);
            newPoint = new Vector3(randPoint.x, 0, randPoint.y);
            foundSuitable = (Vector3.Distance(currentCenter, newPoint) < currentRadius);
            if (totalCountDown < 1)
                return new Vector3(0, -100, 0);  
        }
        return newPoint;
    }

    private int NextZoneTime()
    {
        if (zoneTimesIndex >= ZoneTimes.Count - 1)
        {
            return -1;
        }

        return ZoneTimes[++zoneTimesIndex];
    }

    private float[] ShrinkCircle(float amount)
    {
        float newXR = circle.radii[0] - amount;
        float newYR = circle.radii[1] - amount;
        float[] retVal = new float[2];
        retVal[0] = newXR;
        retVal[1] = newYR;
        return retVal;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(XRadius);
        }
        else
        {
            transform.position = (Vector3)stream.ReceiveNext();
            XRadius = (float)stream.ReceiveNext();
            circle.Draw(Segments, XRadius, XRadius);
            ZoneWall.transform.position = transform.position;
            ZoneWall.transform.localScale = new Vector3((XRadius * 0.01f), 1, (XRadius * 0.01f));
        }
    }
}