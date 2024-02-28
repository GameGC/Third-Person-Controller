using System.Collections;
using System.Threading;
using UnityEngine;

namespace MTPS.Shooter.WeaponsSystem.Extras
{
    public class FramerateTest : MonoBehaviour
    {
        public float Rate = 50.0f;
        float currentFrameTime;

        void Start()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 9999;
            currentFrameTime = Time.realtimeSinceStartup;
            StartCoroutine(WaitForNextFrame());
        }

        IEnumerator WaitForNextFrame()
        {
            var wait = new WaitForEndOfFrame();
            while (true)
            {
                yield return wait;
                currentFrameTime += 1.0f / Rate;
                var t = Time.realtimeSinceStartup;
                var sleepTime = currentFrameTime - t - 0.01f;
                if (sleepTime > 0)
                    Thread.Sleep((int)(sleepTime * 1000));
                while (t < currentFrameTime)
                    t = Time.realtimeSinceStartup;
            }
        }
    }
}
