// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System.Globalization;
using System.Linq;
using Improbable.Unity.Core;
using UnityEngine;

namespace Improbable.Unity.Util
{
    public class MetricsUnityGui : MonoBehaviour
    {
        private Vector2 scrollPosition;
        private bool showUnityMetrics;
        private Texture2D backgroundTexture;
        private GUIStyle backgroundStyle;

        private void Awake()
        {
            backgroundTexture = new Texture2D(1, 1);
            backgroundTexture.SetPixel(0, 0, new Color(0, 0, 0, 0.7f));
            backgroundTexture.Apply();

            backgroundStyle = new GUIStyle { normal = { background = backgroundTexture } };
        }

        private void OnDestroy()
        {
            Destroy(backgroundTexture);
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 50, Screen.width * 0.2f, Screen.height * 0.5f));
            GUILayout.BeginVertical();

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false);

            showUnityMetrics = GUILayout.Toggle(showUnityMetrics, "Metrics");

            if (showUnityMetrics)
            {
                GUILayout.BeginVertical(backgroundStyle);

                var keys = SpatialOS.Metrics.Gauges.Keys.ToList();
                keys.Sort();

                for (var i = 0; i < keys.Count; i++)
                {
                    GUILayout.BeginHorizontal();

                    var key = keys[i];
                    GUILayout.Label(key);
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(SpatialOS.Metrics.Gauges[key].ToString(CultureInfo.InvariantCulture));

                    GUILayout.EndHorizontal();
                }

                GUILayout.EndVertical();
            }

            GUILayout.EndScrollView();

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
