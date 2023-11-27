/*using System;
using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace My.Cool.Assembly
{
    public class AudioClipInspector : Editor
    {
        private PreviewRenderUtility m_PreviewUtility;
        private AudioClip m_Clip;
        private Vector2 m_Position = Vector2.zero;
        private bool m_MultiEditing;
        private static GUIStyle s_PreButton;
        private static Rect s_WantedRect;
        private static bool s_AutoPlay;
        private static bool s_Loop;
        private static bool s_PlayFirst;
        private static AudioClipInspector s_PlayingInstance;
        private static GUIContent s_PlayIcon;
        private static GUIContent s_AutoPlayIcon;
        private static GUIContent s_LoopIcon;
        private static Texture2D s_DefaultIcon;
        private Material m_HandleLinesMaterial;

        private bool playing =>
            (UnityEngine.Object) s_PlayingInstance == (UnityEngine.Object) this &&
            (UnityEngine.Object) m_Clip != (UnityEngine.Object) null && AudioUtil.IsPreviewClipPlaying();

        public override void OnInspectorGUI() => m_MultiEditing = targets.Length > 1;

        private static void Init()
        {
            if (s_PreButton != null) return;
            s_PreButton = (GUIStyle) "preButton";
            s_AutoPlay = EditorPrefs.GetBool("AutoPlayAudio", false);
            s_Loop = false;
            s_AutoPlayIcon = EditorGUIUtility.TrIconContent("preAudioAutoPlayOff", "Turn Auto Play on/off");
            s_PlayIcon = EditorGUIUtility.TrIconContent("PlayButton", "Play");
            s_LoopIcon = EditorGUIUtility.TrIconContent("preAudioLoopOff", "Loop on/off");
            s_DefaultIcon = EditorGUIUtility.Load("Profiler.Audio") as Texture2D;
        }

        public void OnDisable()
        {
            if ((UnityEngine.Object) s_PlayingInstance == (UnityEngine.Object) this)
            {
                AudioUtil.StopAllPreviewClips();
                s_PlayingInstance = (AudioClipInspector) null;
            }

            EditorPrefs.SetBool("AutoPlayAudio", s_AutoPlay);
            if (m_PreviewUtility != null)
            {
                m_PreviewUtility.Cleanup();
                m_PreviewUtility = (PreviewRenderUtility) null;
            }

            m_HandleLinesMaterial = (Material) null;
        }

        public void OnEnable()
        {
            s_AutoPlay = EditorPrefs.GetBool("AutoPlayAudio", false);
            if (s_AutoPlay) s_PlayFirst = true;
            m_HandleLinesMaterial = EditorGUIUtility.LoadRequired("SceneView/HandleLines.mat") as Material;
        }

        public override Texture2D RenderStaticPreview(string assetPath, UnityEngine.Object[] subAssets, int width,
            int height)
        {
            AudioClip target = this.target as AudioClip;
            AudioImporter atPath = AssetImporter.GetAtPath(assetPath) as AudioImporter;
            if ((UnityEngine.Object) atPath == (UnityEngine.Object) null ||
                !ShaderUtil.hardwareSupportsRectRenderTexture)
                return (Texture2D) null;
            if (m_PreviewUtility == null) m_PreviewUtility = new PreviewRenderUtility();
            m_PreviewUtility.BeginStaticPreview(new Rect(0.0f, 0.0f, (float) width, (float) height));
            m_HandleLinesMaterial.SetPass(0);
            DoRenderPreview(false, target, atPath,
                new Rect(0.05f * (float) width * EditorGUIUtility.pixelsPerPoint,
                    0.05f * (float) width * EditorGUIUtility.pixelsPerPoint,
                    1.9f * (float) width * EditorGUIUtility.pixelsPerPoint,
                    1.9f * (float) height * EditorGUIUtility.pixelsPerPoint), 1f);
            return m_PreviewUtility.EndStaticPreview();
        }

        public override bool HasPreviewGUI() => targets != null;

        public override void OnPreviewSettings()
        {
            if ((UnityEngine.Object) s_DefaultIcon == (UnityEngine.Object) null) Init();
            AudioClip target = this.target as AudioClip;
            m_MultiEditing = targets.Length > 1;
            using (new EditorGUI.DisabledScope(m_MultiEditing && !playing))
            {
                bool flag = GUILayout.Toggle(playing, s_PlayIcon, EditorStyles.toolbarButton);
                if (flag != playing)
                {
                    if (flag)
                    {
                        PlayClip(target, loop: s_Loop);
                    }
                    else
                    {
                        AudioUtil.StopAllPreviewClips();
                        m_Clip = (AudioClip) null;
                    }
                }
            }

            using (new EditorGUI.DisabledScope(m_MultiEditing))
            {
                s_AutoPlay = s_AutoPlay && !m_MultiEditing;
                s_AutoPlay = GUILayout.Toggle(s_AutoPlay, s_AutoPlayIcon, EditorStyles.toolbarButton);
            }

            bool loop = s_Loop;
            s_Loop = GUILayout.Toggle(s_Loop, s_LoopIcon, EditorStyles.toolbarButton);
            if (loop == s_Loop || !playing) return;
            AudioUtil.LoopPreviewClip(s_Loop);
        }

        private void PlayClip(AudioClip clip, int startSample = 0, bool loop = false)
        {
            AudioUtil.StopAllPreviewClips();
            AudioUtil.PlayPreviewClip(clip, startSample, loop);
            m_Clip = clip;
            s_PlayingInstance = this;
        }

        private void DoRenderPreview(bool setMaterial, AudioClip clip, AudioImporter audioImporter, Rect wantedRect,
            float scaleFactor)
        {
            scaleFactor *= 0.95f;
            float[] minMaxData = (UnityEngine.Object) audioImporter == (UnityEngine.Object) null
                ? (float[]) null
                : AudioUtil.GetMinMaxData(audioImporter);
            int numChannels = clip.channels;
            int numSamples = minMaxData == null ? 0 : minMaxData.Length / (2 * numChannels);
            float height = wantedRect.height / (float) numChannels;
            for (int channel = 0; channel < numChannels; channel++)
            {
                Rect r = new Rect(wantedRect.x, wantedRect.y + height * (float) channel, wantedRect.width, height);
                Color curveColor = new Color(1f, 0.54901963f, 0.0f, 1f);
                AudioCurveRendering.AudioMinMaxCurveAndColorEvaluator eval =
                    (AudioCurveRendering.AudioMinMaxCurveAndColorEvaluator) ((float x, out Color col,
                        out float minValue, out float maxValue) =>
                    {
                        col = curveColor;
                        if (numSamples <= 0)
                        {
                            minValue = 0.0f;
                            maxValue = 0.0f;
                        }
                        else
                        {
                            int index1 =
                                ((int) Mathf.Floor(Mathf.Clamp(x * (float) (numSamples - 2), 0.0f,
                                    (float) (numSamples - 2))) * numChannels + channel) * 2;
                            int index2 = index1 + numChannels * 2;
                            minValue = Mathf.Min(minMaxData[index1 + 1], minMaxData[index2 + 1]) * scaleFactor;
                            maxValue = Mathf.Max(minMaxData[index1], minMaxData[index2]) * scaleFactor;
                            if ((double) minValue > (double) maxValue)
                            {
                                (minValue, maxValue) = (maxValue, minValue);
                            }
                        }
                    });
                if (setMaterial)
                    AudioCurveRendering.DrawMinMaxFilledCurve(r, eval);
                else
                    AudioCurveRendering.DrawMinMaxFilledCurveInternal(r, eval);
            }
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            if ((UnityEngine.Object) s_DefaultIcon == (UnityEngine.Object) null) Init();
            AudioClip target = this.target as AudioClip;
            Event current = Event.current;
            if (current.type != EventType.Repaint && current.type != EventType.Layout && current.type != EventType.Used)
            {
                switch (current.type)
                {
                    case EventType.MouseDown:
                    case EventType.MouseDrag:
                        if (!r.Contains(current.mousePosition)) break;
                        int num1 = (int) ((double) current.mousePosition.x *
                                          (double) (AudioUtil.GetSampleCount(target) / (int) r.width));
                        if (!AudioUtil.IsPreviewClipPlaying() ||
                            (UnityEngine.Object) target != (UnityEngine.Object) m_Clip)
                            PlayClip(target, num1, s_Loop);
                        else
                            AudioUtil.SetPreviewClipSamplePosition(target, num1);
                        current.Use();
                        break;
                }
            }
            else
            {
                if (Event.current.type == EventType.Repaint) background.Draw(r, false, false, false, false);
                int channelCount = AudioUtil.GetChannelCount(target);
                s_WantedRect = new Rect(r.x, r.y, r.width, r.height);
                float num2 = s_WantedRect.width / target.length;
                if (!AudioUtil.HasPreview(target) && AudioUtil.IsTrackerFile(target))
                {
                    float y = (double) r.height > 150.0
                        ? (float) ((double) r.y + (double) r.height / 2.0 - 10.0)
                        : (float) ((double) r.y + (double) r.height / 2.0 - 25.0);
                    if ((double) r.width > 64.0)
                    {
                        if (AudioUtil.IsTrackerFile(target))
                            EditorGUI.DropShadowLabel(new Rect(r.x, y, r.width, 20f),
                                string.Format("Module file with " + AudioUtil.GetMusicChannelCount(target).ToString() +
                                              " channels."));
                        else
                            EditorGUI.DropShadowLabel(new Rect(r.x, y, r.width, 20f),
                                "Can not show PCM data for this file");
                    }

                    if ((UnityEngine.Object) m_Clip == (UnityEngine.Object) target && playing)
                    {
                        TimeSpan timeSpan = new TimeSpan(0, 0, 0, 0,
                            (int) ((double) AudioUtil.GetPreviewClipPosition() * 1000.0));
                        EditorGUI.DropShadowLabel(new Rect(s_WantedRect.x, s_WantedRect.y, s_WantedRect.width, 20f),
                            string.Format("Playing - {0:00}:{1:00}.{2:000}", (object) timeSpan.Minutes,
                                (object) timeSpan.Seconds, (object) timeSpan.Milliseconds));
                    }
                }
                else
                {
                    PreviewGUI.BeginScrollView(s_WantedRect, m_Position, s_WantedRect,
                        (GUIStyle) "PreHorizontalScrollbar", (GUIStyle) "PreHorizontalScrollbarThumb");
                    if (Event.current.type == EventType.Repaint)
                        DoRenderPreview(true, target, AudioUtil.GetImporterFromClip(target), s_WantedRect, 1f);
                    for (int index = 0; index < channelCount; ++index)
                    {
                        if (channelCount > 1 && (double) r.width > 64.0)
                            EditorGUI.DropShadowLabel(
                                new Rect(s_WantedRect.x + 5f,
                                    s_WantedRect.y + s_WantedRect.height / (float) channelCount * (float) index, 30f,
                                    20f), "ch " + (index + 1).ToString());
                    }

                    if ((UnityEngine.Object) m_Clip == (UnityEngine.Object) target && playing)
                    {
                        float previewClipPosition = AudioUtil.GetPreviewClipPosition();
                        TimeSpan timeSpan = new TimeSpan(0, 0, 0, 0, (int) ((double) previewClipPosition * 1000.0));
                        GUI.DrawTexture(
                            new Rect(s_WantedRect.x + (float) (int) ((double) num2 * (double) previewClipPosition),
                                s_WantedRect.y, 2f, s_WantedRect.height), (Texture) EditorGUIUtility.whiteTexture);
                        if ((double) r.width > 64.0)
                            EditorGUI.DropShadowLabel(new Rect(s_WantedRect.x, s_WantedRect.y, s_WantedRect.width, 20f),
                                string.Format("{0:00}:{1:00}.{2:000}", (object) timeSpan.Minutes,
                                    (object) timeSpan.Seconds, (object) timeSpan.Milliseconds));
                        else
                            EditorGUI.DropShadowLabel(new Rect(s_WantedRect.x, s_WantedRect.y, s_WantedRect.width, 20f),
                                string.Format("{0:00}:{1:00}", (object) timeSpan.Minutes, (object) timeSpan.Seconds));
                    }

                    PreviewGUI.EndScrollView();
                }

                if (!m_MultiEditing &&
                    (s_PlayFirst || s_AutoPlay && (UnityEngine.Object) m_Clip != (UnityEngine.Object) target))
                {
                    PlayClip(target, loop: s_Loop);
                    s_PlayFirst = false;
                }

                if (!playing) return;
                GUIView.current.Repaint();
            }
        }

        public override string GetInfoString()
        {
            AudioClip target = this.target as AudioClip;
            int channelCount = AudioUtil.GetChannelCount(target);
            int num;
            string str1;
            switch (channelCount)
            {
                case 1:
                    str1 = "Mono";
                    break;
                case 2:
                    str1 = "Stereo";
                    break;
                default:
                    num = channelCount - 1;
                    str1 = num.ToString() + ".1";
                    break;
            }

            string str2 = str1;
            AudioCompressionFormat compressionFormat1 = AudioUtil.GetTargetPlatformSoundCompressionFormat(target);
            AudioCompressionFormat compressionFormat2 = AudioUtil.GetSoundCompressionFormat(target);
            string str3 = compressionFormat1.ToString();
            if (compressionFormat1 != compressionFormat2)
                str3 = str3 + " (" + compressionFormat2.ToString() + " in editor)";
            string[] strArray = new string[6] {str3, ", ", null, null, null, null};
            num = AudioUtil.GetFrequency(target);
            strArray[2] = num.ToString();
            strArray[3] = " Hz, ";
            strArray[4] = str2;
            strArray[5] = ", ";
            string str4 = string.Concat(strArray);
            TimeSpan timeSpan = new TimeSpan(0, 0, 0, 0, (int) AudioUtil.GetDuration(target));
            string infoString;
            if ((uint) AudioUtil.GetDuration(target) == uint.MaxValue)
                infoString = str4 + "Unlimited";
            else
                infoString = str4 + UnityStringFormat("{0:00}:{1:00}.{2:000}", (object) timeSpan.Minutes,
                    (object) timeSpan.Seconds, (object) timeSpan.Milliseconds);
            return infoString;
        }

        private static string UnityStringFormat(string fmt, params object[] args)
        {
            return string.Format(CultureInfo.InvariantCulture.NumberFormat, fmt, args);
        }
    }

}*/