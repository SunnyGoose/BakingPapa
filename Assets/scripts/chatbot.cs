using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class chatbot : MonoBehaviour
{
    [Header("API Settings")]
    [SerializeField] private string geminiApiKey = "AIzaSyBrxe635UX7dYCj9f7v8oV_ToiXXGj0I_U";
    private string geminiEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";

    [Header("UI References")]
    [SerializeField] private GameObject popupPanel;       // assign in Inspector
    [SerializeField] private Text chatOutputText;         // text inside ScrollView for recipes
    [SerializeField] private Button papaButton;           // cat avatar button
    [SerializeField] private Dropdown timeDropdown;       // dropdown for time
    [SerializeField] private Dropdown difficultyDropdown; // dropdown for difficulty
    [SerializeField] private Button confirmButton;       // confirm selections button

    public List<string> foodList = new List<string>();

    private string selectedTime;
    private string selectedDifficulty;

    private void Start()
    {
        // Hide popup panel at start
        popupPanel.SetActive(false);

        // Hook Papa button
        papaButton.onClick.AddListener(OpenPopup);

        // Hook dropdowns
        if (timeDropdown != null)
            timeDropdown.onValueChanged.AddListener(OnTimeChanged);

        if (difficultyDropdown != null)
            difficultyDropdown.onValueChanged.AddListener(OnDifficultyChanged);

        // Hook confirm button
        if (confirmButton != null)
            confirmButton.onClick.AddListener(ConfirmSelections);

        // Initialize selections with first option
        if (timeDropdown != null) selectedTime = timeDropdown.options[timeDropdown.value].text;
        if (difficultyDropdown != null) selectedDifficulty = difficultyDropdown.options[difficultyDropdown.value].text;
    }

    // --- Popup ---
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
        Debug.Log("Selected time: " + selectedTime);
    }
    public void SelectDifficulty(string difficulty)
    {
        selectedDifficulty = difficulty;
        Debug.Log("Selected difficulty: " + selectedDifficulty);
    }


    // --- Dropdown Handlers ---
    private void OnTimeChanged(int index)
    {
        selectedTime = timeDropdown.options[index].text;
        Debug.Log($"Time dropdown index: {index}, value: {selectedTime}");
    }

    private void OnDifficultyChanged(int index)
    {
        selectedDifficulty = difficultyDropdown.options[index].text;
        Debug.Log($"Difficulty dropdown index: {index}, value: {selectedDifficulty}");
    }


    // --- Confirm selections ---
    private void ConfirmSelections()
    {
        popupPanel.SetActive(false);
        RequestRecipes();
    }

    // --- Gemini API Request ---
    private void RequestRecipes()
    {
        StartCoroutine(SendRequestToGemini(foodList));
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

        string json = JsonUtility.ToJson(requestBody);

        using (UnityWebRequest www = new UnityWebRequest(geminiEndpoint, "POST"))
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
                chatOutputText.text = responseText;
            }
        }
    Debug.Log("Sending JSON to Gemini:\n" + json);
    Debug.Log("Endpoint: " + geminiEndpoint);

    }
    
}
