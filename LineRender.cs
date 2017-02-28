using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRender : MonoBehaviour {

   public LineRenderer LR;
   public int n;
   public float r;
   private Vector3 center;

   public Material red;
   public Material green;
   public Material blue;

   private bool isXCircle = false;
   private bool isYCircle = false;
   private bool isZCircle = false;

   void Start ()
   {
       LR = GetComponent<LineRenderer>();
   }		
   public void DrawXCycline()
   {
       if (isXCircle) return;

       isXCircle = true;
       isYCircle = false;
       isZCircle = false;

       LR.material = red;
       center = transform.position;
       LR = this.GetComponent<LineRenderer>();//得到组件
       LR.numPositions = n + 1;//设置线的段数
       float y;
       float z;
       //循环着取出36个点
       for (int i = 0; i < n + 1; i++)
       {
           y = Mathf.Sin((360f * i / n) * Mathf.Deg2Rad) * r;//横坐标
           z = Mathf.Cos((360f * i / n) * Mathf.Deg2Rad) * r;//纵坐标
           LR.SetPosition(i, new Vector3(center.x, center.y + y, center.z + z));
       }
   }
   public void DrawYCycline()
   {
       if (isYCircle) return;

       isXCircle = false;
       isYCircle = true;
       isZCircle = false;

       LR.material = green;
       center = transform.position;
       LR = this.GetComponent<LineRenderer>();//得到组件
       LR.numPositions = n + 1;//设置线的段数
       float x;
       float z;
       //循环着取出36个点
       for (int i = 0; i < n + 1; i++)
       {
           LR.startColor = new Color(1, 0, 0);
           LR.endColor = new Color(1, 0, 0);
           x = Mathf.Sin((360f * i / n) * Mathf.Deg2Rad) * r;//横坐标
           z = Mathf.Cos((360f * i / n) * Mathf.Deg2Rad) * r;//纵坐标
           LR.SetPosition(i, new Vector3(center.x + x, center.y, center.z + z));
       }
   }
   public void DrawZCycline()
   {
       if (isZCircle) return;

       isXCircle = false;
       isYCircle = false;
       isZCircle = true;

       LR.material = blue;
       center = transform.position;
       LR = this.GetComponent<LineRenderer>();//得到组件
       LR.numPositions = n + 1;//设置线的段数
       float x;
       float y;
       //循环着取出36个点
       for (int i = 0; i < n + 1; i++)
       {
           x = Mathf.Sin((360f * i / n) * Mathf.Deg2Rad) * r;//横坐标
           y = Mathf.Cos((360f * i / n) * Mathf.Deg2Rad) * r;//纵坐标
           LR.SetPosition(i, new Vector3(center.x + x, center.y + y, center.z));
       }
   }
}
