using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using TMPro;
using static System.Net.WebRequestMethods;

public class NewBehaviourScript : MonoBehaviour
{
    // Визуализация.
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI valueText;
    public TextMeshProUGUI soundText;

    private AudioSource selectAudio;

    // Звуки.
    public AudioClip goodSpeak;
    public AudioClip normalSpeak;
    public AudioClip badSpeak;

    private List<(string level, float value, int soundType)> dataSet = new List<(string level, float value, int soundType)>();
    private bool statusStart = false;
    private int i = 1;

    // URL для доступа к таблице Google Sheets.
    private const string URL = "СЮДА ВСТАВИТ !!!!";

    private void Start () {
        StartCoroutine(GoogleSheets());
    }

    private void Update () {
        if (!statusStart && i < dataSet.Count) {
            var data = dataSet[i];
            string level = data.level;
            float value = data.value;
            int soundType = data.soundType;

            AudioClip clip = soundType switch {
                1 => badSpeak,
                2 => normalSpeak,
                3 => goodSpeak,
                _ => null
            };

            if (clip != null) {
                StartCoroutine(PlaySelectAudio(clip, 3, level, value));
            }
        }
    }

    private IEnumerator GoogleSheets () {
        UnityWebRequest curentResp = UnityWebRequest.Get(URL);
        yield return curentResp.SendWebRequest();

        if (curentResp.result == UnityWebRequest.Result.Success) {
            string rawResp = curentResp.downloadHandler.text;
            Debug.Log("Raw Response: " + rawResp); // Отладочное сообщение

            var rawJson = JSON.Parse(rawResp);
            Debug.Log("Parsed JSON: " + rawJson.ToString()); // Отладочное сообщение

            foreach (var itemRawJson in rawJson["values"]) {
                var parseJson = JSON.Parse(itemRawJson.ToString());
                var selectRow = parseJson[0].AsStringList;

                if (selectRow.Count >= 3) {
                    string level = selectRow[1]; // Значение из второго столбца
                    float value = float.Parse(selectRow[2]); // Значение из третьего столбца
                    int soundType = int.Parse(selectRow[2]); // Значение из третьего столбца для определения типа звука
                    dataSet.Add((level, value, soundType));
                }
                else {
                    Debug.LogWarning("Row does not have enough columns: " + selectRow.ToString());
                }
            }
        }
        else {
            Debug.LogError("Error: " + curentResp.error);
        }
    }

    private IEnumerator PlaySelectAudio (AudioClip clip, float duration, string level, float value) {
        statusStart = true;
        selectAudio = GetComponent<AudioSource>();
        selectAudio.clip = clip;
        selectAudio.Play();

        // Обновляем текст для отображения названия аудио и значения.
        soundText.text = "Звук: " + clip.name + ".mp3";
        valueText.text = "Значение: " + value.ToString();
        levelText.text = "Уровень: " + level;

        yield return new WaitForSeconds(duration);
        statusStart = false;
        i++;
    }
}