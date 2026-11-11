using UnityEngine;
using System.Diagnostics;
using System.Text.RegularExpressions;

public class DialogueVoiceSynthesizer : MonoBehaviour
{
    public static DialogueVoiceSynthesizer Instance;

    [Header("Voice Settings")]
    public bool enableVoiceActing = true;
    public bool playRadioClickBeep = true;
    [Range(0.1f, 1f)] public float radioBeepVolume = 0.25f;

    private Process activeSpeechProcess;
    private AudioSource radioBeepSource;
    private AudioClip generatedRadioBeepClip;
    private bool wasPaused = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        SetupProceduralRadioBeep();
    }

    void Update()
    {
        // אם השחקן פתח תפריט (ESC) או שהמשחק הסתיים - עוצרים מיד את הדיבור הפעיל
        bool isPausedOrEnded = !MenuAndGameManager.GameHasStarted || Time.timeScale == 0f || HealthSystem.MatchEnded;
        if (isPausedOrEnded && !wasPaused)
        {
            StopCurrentVoice();
            wasPaused = true;
        }
        else if (!isPausedOrEnded && wasPaused)
        {
            wasPaused = false;
        }
    }

    // הפונקציה הראשית שמקבלת את שם הדובר ואת הטקסט ומדבבת אותו בקול המתאים!
    public void SpeakLine(string speakerName, string rawDialogueText)
    {
        if (!enableVoiceActing || string.IsNullOrWhiteSpace(rawDialogueText)) return;

        StopCurrentVoice();

        // 1. השמעת צליל פתיחת קשר ("צפצוף מכשיר קשר" עדין)
        if (playRadioClickBeep && radioBeepSource != null && generatedRadioBeepClip != null)
        {
            radioBeepSource.PlayOneShot(generatedRadioBeepClip, radioBeepVolume);
        }

        // 2. ניקוי הטקסט מתגיות עיצוב ומסוגריים של מקשים כמו [E] או [1] כדי שהקריין יקרא טבעי
        string cleanText = CleanTextForSpeech(rawDialogueText);
        if (string.IsNullOrWhiteSpace(cleanText)) return;

        // 3. בחירת קול ומהירות דיבור לפי הדמות המדברת
        string voiceName = "Alex";
        int wordsPerMinute = 190;

        string spk = speakerName.ToLower();
        if (spk.Contains("maya") || spk.Contains("clerk") || spk.Contains("teller") || spk.Contains("hostage"))
        {
            // קול נשי לשוטרת מאיה ולפקידה בדלפק (מהיר ובהול יותר אם זו הפקידה)
            voiceName = "Samantha";
            wordsPerMinute = spk.Contains("clerk") ? 215 : 195;
        }
        else if (spk.Contains("daniel"))
        {
            // קול גברי צעיר ומתוח לאח הצעיר דניאל
            voiceName = "Daniel";
            wordsPerMinute = 205;
        }
        else if (spk.Contains("max") || spk.Contains("viktor"))
        {
            // קול גברי מחוספס לחבר הצוות מקס
            voiceName = "Fred";
            wordsPerMinute = 185;
        }
        else if (spk.Contains("roi") || spk.Contains("swat") || spk.Contains("radio") || spk.Contains("dispatch"))
        {
            // קול סמכותי לשוטר הוותיק רועי ולמוקד המשטרתי
            voiceName = "Tom";
            wordsPerMinute = 195;
        }

        StartMacOSSpeechProcess(voiceName, wordsPerMinute, cleanText);
    }

    private void StartMacOSSpeechProcess(string voice, int rateWPM, string textToSpeak)
    {
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        try
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "/usr/bin/say",
                Arguments = $"-v {voice} -r {rateWPM} \"{textToSpeak}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            activeSpeechProcess = Process.Start(psi);
        }
        catch
        {
            // גיבוי לקול ברירת המחדל של המערכת אם אחד הקולות לא מותקן
            try
            {
                ProcessStartInfo fallbackPsi = new ProcessStartInfo
                {
                    FileName = "/usr/bin/say",
                    Arguments = $"-r {rateWPM} \"{textToSpeak}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                activeSpeechProcess = Process.Start(fallbackPsi);
            }
            catch { }
        }
#endif
    }

    private string CleanTextForSpeech(string input)
    {
        // הסרת תגיות HTML/TextMeshPro כמו <b> או <color=...>
        string cleaned = Regex.Replace(input, "<.*?>", string.Empty);

        // הסרת הערות סוגריים ארוכות בסוף משפט כמו (Press [T] anytime to talk)
        cleaned = Regex.Replace(cleaned, @"\([^\)]*\)", string.Empty);

        // הפיכת סימני מקשים כמו [F] או [E] למילה פשוטה
        cleaned = cleaned.Replace("[", "").Replace("]", "");

        // ניקוי מרכאות כפולות או תווים מיוחדים שעלולים לשבור את שורת הפקודה
        cleaned = cleaned.Replace("\"", "").Replace("'", "").Replace("\n", " ");

        return cleaned.Trim();
    }

    public void StopCurrentVoice()
    {
        if (activeSpeechProcess != null)
        {
            try
            {
                if (!activeSpeechProcess.HasExited)
                {
                    activeSpeechProcess.Kill();
                }
            }
            catch { }
            activeSpeechProcess = null;
        }
    }

    // מייצר בקוד צליל "צפצוף מכשיר קשר" קצר ונקי בלי צורך בקובץ שמע חיצוני
    private void SetupProceduralRadioBeep()
    {
        radioBeepSource = gameObject.AddComponent<AudioSource>();
        radioBeepSource.playOnAwake = false;
        radioBeepSource.spatialBlend = 0f;

        int sampleRate = 44100;
        int sampleCount = sampleRate / 12; // ~80ms beep
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            // שילוב שני תדרים שיוצרים צליל PTT של מכשיר קשר משטרתי
            float wave = Mathf.Sin(2f * Mathf.PI * 1850f * t) * 0.5f + Mathf.Sin(2f * Mathf.PI * 2400f * t) * 0.5f;
            float envelope = Mathf.Clamp01(1f - ((float)i / sampleCount));
            samples[i] = wave * envelope * 0.4f;
        }

        generatedRadioBeepClip = AudioClip.Create("TacticalRadioBeep", sampleCount, 1, sampleRate, false);
        generatedRadioBeepClip.SetData(samples, 0);
    }

    void OnDisable()
    {
        StopCurrentVoice();
    }

    void OnDestroy()
    {
        StopCurrentVoice();
    }
}