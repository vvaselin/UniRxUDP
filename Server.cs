using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UniRx;
using UnityEngine.UI;
using System;
//using UnityEditor.Search;
using System.Collections.Generic;
using System.Linq;

using System.IO;

public class ServerExample : MonoBehaviour
{
    private UdpClient udpClient;
    private Subject<string> subject = new Subject<string>();
    private string message_;

    Queue<float> queue = new Queue<float>();
    Vector2 vec2 = new Vector2(0, 0);

    Vector2 BaseVec = new Vector2(0, 0);

    void Start()
    {
        queue.Enqueue(0);

        udpClient = new UdpClient(9000);
        udpClient.BeginReceive(OnReceived, udpClient);

        subject
            .ObserveOnMainThread()
            .Subscribe(msg => {

                float tmp = float.Parse(msg);
                //キューに保存
                queue.Enqueue(tmp);
                //ベクトルの合成
                vec2 += getVec(tmp);

                //数フレーム分だけ残す
                if (queue.Count > 10) vec2 -= getVec(queue.Dequeue());

                var result = Quaternion.Euler(0, 0, 90) * vec2;

                //ベクトルから角度を計算
                float magne = -(Mathf.Atan2(result.x, result.y) * Mathf.Rad2Deg);
                //-180～180なので補正
                if (magne < 0) magne += 360;

                transform.rotation = Quaternion.Euler(0, -magne, 0);

                message_ = magne.ToString();

            }).AddTo(this);
    }

    private void OnReceived(System.IAsyncResult result)
    {
        UdpClient getUdp = (UdpClient)result.AsyncState;
        IPEndPoint ipEnd = null;

        byte[] getByte = getUdp.EndReceive(result, ref ipEnd);

        var message = Encoding.UTF8.GetString(getByte);
        //Debug.Log(message);
        subject.OnNext(message);

        getUdp.BeginReceive(OnReceived, getUdp);
    }

    private void OnDestroy()
    {
        udpClient.Close();
    }
    private Vector2 getVec(float angle)
    {
        return new Vector2(Mathf.Cos(angle * Mathf.PI / 180), Mathf.Sin(angle * Mathf.PI / 180));
    }

    private void GenFile(string path)
    {
        string stepCntString = "";
        if (!File.Exists(path))
        {
            StreamWriter sw = new StreamWriter(path, true);    // ファイル書き込み指定
            sw.WriteLine(stepCntString);                           // 情報を書き込み
            sw.Close();
        }
    }

    private void WriteFile(string message, string path)
    {
        StreamWriter sw = new StreamWriter(path, true);
        sw.WriteLine(message);
        sw.Flush();
        sw.Close();
    }

}
