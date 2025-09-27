using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class PapaManager : MonoBehaviour
{
    [Header("API Settings")]
    [SerializeField] private string geminiApiKey = "AIzaSyBrxe635UX7dYCj9f7v8oV_ToiXXGj0I_U";
    [SerializeField] private string geminiEndpoint = "https://generativelanguage.googleapis.com/v1beta2/models/YOUR_MODEL:generateContent";

    [Header("UI References")]
    [SerializeField] private GameObject popupPanel;       // assign in Inspector
    [SerializeField] private Text chatOutputText;         // text inside ScrollView for recipes
    [SerializeField] private Button papaButton;           // cat avatar button

    private string selectedTime;
    private string selectedDifficulty;

    public List<string> foodList = new List<string>();

    private void Start()
    {
        // make sure popup is hidden at start
        popupPanel.SetActive(false);

        // hook Papa button to open popup
        papaButton.onClick.AddListener(OpenPopup);
    }

    // --- Popup controls ---
    public void OpenPopup()
    {
        popupPanel.SetActive(true);
    }

    public void ClosePopup()
    {
        popupPanel.SetActive(false);
    }

    public void SelectTime(string time)
    {
        selectedTime = time;
        Debug.Log("Selected time: " + time);
    }

    public void SelectDifficulty(string difficulty)
    {
        selectedDifficulty = difficulty;
        Debug.Log("Selected difficulty: " + difficulty);
    }

    public void ConfirmSelections()
    {
        popupPanel.SetActive(false);
        RequestRecipes();
    }

    // --- Gemini request ---
    public void RequestRecipes()
    {
        try
        {
            StartCoroutine(SendRequestToGemini(foodList));
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error starting Gemini request: " + ex.Message);
        }
    }

    private IEnumerator SendRequestToGemini(List<string> foods)
    {
        string prompt = $"Here are the foods I have: {string.Join(", ", foods)}.\n" +
                        $"I have about {selectedTime} minutes and I want a {selectedDifficulty} recipe.\n" +
                        "Please suggest 3 baking recipes I can make using only these ingredients, and list all the steps and measurements.";

        var requestBody = new
        {
            model = "gemini-2.5-flash",
            prompt = prompt,
            temperature = 0.7f,
            maxTokens = 300
        };

        string json;
        try
        {
            json = JsonUtility.ToJson(requestBody);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error serializing request body: " + ex.Message);
            yield break;
        }

        using (UnityWebRequest www = new UnityWebRequest(geminiEndpoint, "POST"))
        {
            try
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
                www.SetRequestHeader("Authorization", "Bearer " + geminiApiKey);

                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Gemini request error: " + www.error);
                }
                else
                {
                    string responseText = www.downloadHandler.text;
                    Debug.Log("Received: " + responseText);

                    try
                    {
                        ProcessAndShowRecipes(responseText);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError("Error processing Gemini response: " + ex.Message);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Unexpected error during Gemini request: " + ex.Message);
            }
        }
    }

    private void ProcessAndShowRecipes(string jsonResponse)
    {
        try
        {
            // For now, just dump raw response text into chat box
            chatOutputText.text = jsonResponse;
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error parsing response JSON: " + ex.Message);
        }
    }
}
