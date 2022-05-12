using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using NativeWebSocket;
using UnityEngine.UI;
using LitJson;
using System;

public class Ui : MonoBehaviour
{
    [SerializeField] string websocketAdress;

    [SerializeField] TMP_InputField username;
    [SerializeField] TMP_InputField room_name;

    WebSocket websocket;

    const int player_col_count = 4;
    const int upper_score_row_count = 6;
    const int per_yahtzee_bonus_score = 100;

    [SerializeField] Transform[] otherDice;

    [SerializeField] Transform ownDice;

    [SerializeField] Transform[] upper_scores;
    [SerializeField] Transform upperTotals;
    [SerializeField] Transform upperBoni;
    [SerializeField] Transform upperBoni_total;

    [SerializeField] Transform[] lower_scores;
    [SerializeField] Transform yahtzee_boni;
    [SerializeField] Transform yahtzee_boni_scores;
    [SerializeField] Transform upper_total;
    [SerializeField] Transform lower_total;
    [SerializeField] Transform grand_total;

    [SerializeField] TMP_Text[] player_1_texts;
    [SerializeField] TMP_Text[] player_2_texts;
    [SerializeField] TMP_Text[] player_3_texts;
    [SerializeField] TMP_Text[] player_4_texts;

    [SerializeField] EnableDisableListener room_screen;
    [SerializeField] Transform room_scroll_content;
    [SerializeField] Transform room_button_prefab;

    [SerializeField] Transform in_game_screen;

    [SerializeField] Sprite[] dice_sprites;

    [SerializeField] Color selected_color;
    [SerializeField] Color unselected_color;
    [SerializeField] Color selected_color_half;
    [SerializeField] Color unselected_color_half;

    [SerializeField] TMP_Text warning_text;
    [SerializeField] TMP_Text winning_text;

    [SerializeField] GameObject[] other_player_cards;

    List<Image[]> dice;
    List<int[]> dice_values;
    List<TMP_Text[]> score_text_fields;
    List<int[]> scores;
    int[] upper_sums;
    int[] grand_totals;

    TMP_Text[] upper_totals_texts;
    TMP_Text[] upper_boni_texts;
    TMP_Text[] upper_boni_total_texts;

    TMP_Text[] lower_yahtzee_boni_texts;
    TMP_Text[] lower_yahtzee_boni_score_texts;
    TMP_Text[] lower_upper_totals;
    TMP_Text[] lower_lower_totals;
    TMP_Text[] lower_grand_totals;

  

    List<int> selected_dice;
    bool notRolled;

    Dictionary<string, int> player_id_dictionary;


    public void Open_Github()
    {
        Application.OpenURL("https://github.com/by-Tobd/yahtzee");
    }

    private void UpdateScores()
    {
        for (int playerI = 0; playerI < player_col_count; playerI++)
        {
            int upperTotal = 0;
            int lowerTotal = 0;
            for (int i = 0; i < score_text_fields[playerI].Length; i++)
            {
                int currScore = scores[playerI][i];
                score_text_fields[playerI][i].text = (currScore == -1) ? "" : currScore.ToString();
                if (i < upper_score_row_count)
                {
                    upperTotal += (currScore == -1) ? 0 : currScore;
                } else
                {
                    lowerTotal += (currScore == -1) ? 0 : currScore;
                }            
            }

            upper_totals_texts[playerI].text = upperTotal.ToString();
            lower_yahtzee_boni_texts[playerI].text = new string('X', scores[playerI][13]);
            lower_yahtzee_boni_score_texts[playerI].text = (per_yahtzee_bonus_score * scores[playerI][13]).ToString();
            lower_upper_totals[playerI].text = (upper_sums[playerI] == -1) ? "" : upper_sums[playerI].ToString();
            lower_lower_totals[playerI].text = lowerTotal.ToString();

            lower_grand_totals[playerI].text = (grand_totals[playerI] == -1) ? "" : grand_totals[playerI].ToString();

            upper_boni_total_texts[playerI].text = (upper_sums[playerI] == -1) ? "" : upper_sums[playerI].ToString();
            upper_boni_texts[playerI].text = (upper_sums[playerI] == -1) ? "" : (upper_sums[playerI] - upperTotal).ToString();
        }
    }

    void UpdateDice()
    {
        Color transparent = new Color(0f, 0f, 0f, 0f);
        for (int playerI = 0; playerI < player_col_count; playerI++)
        {


            for (int i = 0; i < dice[playerI].Length; i++)
            {
                if (!player_id_dictionary.ContainsValue(playerI))
                    dice[playerI][i].color = transparent;
                else
                {
                    int index = dice_values[playerI][i] - 1;
                    if (index >= 0)
                        dice[playerI][i].sprite = dice_sprites[index];

                    if (playerI == 0 && notRolled)
                        dice[playerI][i].color = selected_dice.Contains(i) ? selected_color_half : unselected_color_half;
                    else if (playerI == 0)
                        dice[playerI][i].color = selected_dice.Contains(i) ? selected_color : unselected_color;
                    else
                        dice[playerI][i].color = unselected_color;
                }
                    


            }

        }
    }



    private void UpdateRoomList(JsonData data)
    {
        for (int i = 0; i < room_scroll_content.childCount; i++)
        {
            Destroy(room_scroll_content.GetChild(i).gameObject);
        }
        for (int i = 0; i < data.Count; i++)
        {
            var btn = Instantiate(room_button_prefab, room_scroll_content);
            TMP_Text text = btn.transform.GetComponentInChildren<TMP_Text>();
            text.text = (string)data[i]["name"];
            btn.GetComponent<Button>().onClick.AddListener(() => JoinRoom(text.text));

        }
    }

    public void CreateRoom()
    {
        JsonData data = new JsonData();
        data["room_name"] = room_name.text;
        data["username"] = username.text;

        SendMessage("create_room", data);
    }


    private void SelectDice(int index)
    {
        print("Dice: " + index.ToString());
        if (selected_dice.Contains(index)) 
            selected_dice.Remove(index);
        else
            selected_dice.Add(index);
        UpdateDice();
    }

    public void Roll()
    {
        JsonData data = new JsonData();
        if (!notRolled)
        {
            for (int i = 0; i < selected_dice.Count; i++)
                data.Add(selected_dice[i]);
        } else
        {
            data = new JsonData(2);
        }
        selected_dice = new List<int>();

        UpdateDice();
        SendMessage("roll", data);
    }


    public void SetupVars()
    {

        dice = new List<Image[]>();
        dice.Add(new Image[5]);
        for (int i = 0; i < ownDice.childCount; i++)
        {
            Transform child = ownDice.GetChild(i);
            dice[0][i] = child.GetComponent<Image>();
            int index = i;
            child.GetComponent<Button>().onClick.AddListener(() => SelectDice(index));
            
        }

        for (int i = 0; i < otherDice.Length; i++)
        {
            dice.Add(new Image[5]);
            for (int childI = 0; childI < otherDice[i].childCount; childI++)
            {
                dice[i + 1][childI] = otherDice[i].GetChild(childI).GetComponent<Image>();
            }
        }

        score_text_fields = new List<TMP_Text[]>();
        scores = new List<int[]>();
        dice_values = new List<int[]>();
        for (int playerI = 0; playerI < upper_scores.Length; playerI++)
        {
            scores.Add(new int[14]);
            dice_values.Add(new int[5]);
            for (int i = 0; i < scores[playerI].Length; i++)
            {
                scores[playerI][i] = -1;    
            }
            scores[playerI][13] = 0;

            score_text_fields.Add(new TMP_Text[13]);
            for (int i = 0; i < upper_scores[playerI].childCount; i++)
            {
                int index = i;
                score_text_fields[playerI][i] = upper_scores[playerI].GetChild(i).GetComponentInChildren<TMP_Text>();
                if (playerI == 0)
                    upper_scores[playerI].GetChild(i).GetComponent<Button>().onClick.AddListener(() => SelectScoreRow(index));
            }

            for (int i = 0; i < lower_scores[playerI].childCount; i++)
            {
                int index = i + upper_scores[playerI].childCount;
                score_text_fields[playerI][index] = lower_scores[playerI].GetChild(i).GetComponentInChildren<TMP_Text>();
                if (playerI == 0)
                    lower_scores[playerI].GetChild(i).GetComponent<Button>().onClick.AddListener(() => SelectScoreRow(index));
            }
        }


        upper_totals_texts             = new TMP_Text[4];
        upper_boni_texts               = new TMP_Text[4];
        upper_boni_total_texts         = new TMP_Text[4];
        lower_yahtzee_boni_texts       = new TMP_Text[4];
        lower_yahtzee_boni_score_texts = new TMP_Text[4];
        lower_upper_totals             = new TMP_Text[4];
        lower_lower_totals             = new TMP_Text[4];
        lower_grand_totals             = new TMP_Text[4];


        for (int i = 0; i < player_col_count; i++)
        {

            upper_totals_texts[i] =             upperTotals.GetChild(i).GetComponentInChildren<TMP_Text>();
            upper_boni_texts[i] =               upperBoni.GetChild(i).GetComponentInChildren<TMP_Text>();
            upper_boni_total_texts[i] =         upperBoni_total.GetChild(i).GetComponentInChildren<TMP_Text>();
            lower_yahtzee_boni_texts[i] =       yahtzee_boni.GetChild(i).GetComponentInChildren<TMP_Text>();
            lower_yahtzee_boni_score_texts[i] = yahtzee_boni_scores.GetChild(i).GetComponentInChildren<TMP_Text>();
            lower_upper_totals[i] =             upper_total.GetChild(i).GetComponentInChildren<TMP_Text>();
            lower_lower_totals[i] =             lower_total.GetChild(i).GetComponentInChildren<TMP_Text>();
            lower_grand_totals[i] =             grand_total.GetChild(i).GetComponentInChildren<TMP_Text>();
        }

        selected_dice = new List<int>();
        player_id_dictionary = new Dictionary<string, int>();

        if (room_screen != null)
            room_screen.gameObject.SetActive(true);
        in_game_screen.gameObject.SetActive(false);
        notRolled = true;

        upper_sums = new int[player_col_count];
        grand_totals = new int[player_col_count];

        for (int i = 0; i < player_col_count; i++)
        {
            upper_sums[i] = -1;
            grand_totals[i] = -1;
        }

        winning_text.gameObject.SetActive(false);

        for (int i = 0; i < other_player_cards.Length; i++)
        {
            other_player_cards[i].SetActive(true);
        }
    }                                  

    private void SelectScoreRow(int row)
    {
        JsonData data = new JsonData(row);
        print("SENDASERJ");
        SendMessage("end_turn", data);
    }

    private void Start()
    {
        SetupVars();
        Connect();
        room_screen.isEnabledEvent.AddListener(SetupVars);
        room_screen.isEnabledEvent.AddListener(RequestRoomList);
    }
    
    void RemoveWarning()
    {
        warning_text.alpha = 0f;
    }

    void DisplayWarning(string message)
    {
        Debug.Log(message);
        warning_text.text = GetWarningEnglish(message);
        warning_text.alpha = 1f;
        Invoke("RemoveWarning", 2f);

    }

    void HandleScoreInput(JsonData data)
    {
        string user = (string)data["player"];
        if (user == username.text)
            notRolled = true;
        int user_id = player_id_dictionary[user];
        
        int[] score = new int[data["score"].Count];

        for (int i = 0; i < data["score"].Count; i++)
        {
            score[i] = (int)data["score"][i];
        }

        scores[user_id] = score;

        UpdateScores();
        UpdateDice();
    }


    private void JoinRoom(string name)
    {
        if (websocket == null) {
            Connect();
            return;
        }
        JsonData data = new JsonData();
        data["room_name"] = name;
        data["username"] = username.text;
        SendMessage("join_room", data);
    }

    public void RequestRoomList()
    {
        if (websocket != null)
            SendMessage("list_rooms", new JsonData(""));
        else
            Connect();
    }

    void OnRoomUpdate(JsonData data)
    {

        for (int i = 0; i < data["players"].Count; i++)
        {
            if ((string)data["players"][i]["name"] == username.text)
                data["players"].Remove(data["players"][i]);
        }


        foreach (var tex in player_1_texts)
            tex.text = username.text;
        foreach (var tex in player_2_texts)
            tex.text = (data["players"].Count > 0) ? (string)data["players"][0]["name"] : "";
        foreach (var tex in player_3_texts)
            tex.text = (data["players"].Count > 1) ? (string)data["players"][1]["name"] : "";
        foreach (var tex in player_4_texts)
            tex.text = (data["players"].Count > 2) ? (string)data["players"][2]["name"] : "";
        player_id_dictionary = new Dictionary<string, int>();

        player_id_dictionary[username.text] = 0;

        for (int playerI = 0; playerI < data["players"].Count; playerI++)
        {
            player_id_dictionary[(string)data["players"][playerI]["name"]] = playerI + 1;
            int[] newScore = new int[data["players"][playerI]["score"].Count];
            for (int i = 0; i < newScore.Length; i++)
                newScore[i] = (int)data["players"][playerI]["score"][i];

            int[] newDice = new int[data["players"][playerI]["dice"].Count];
            for (int i = 0; i < newDice.Length; i++)
                newDice[i] = (int)data["players"][playerI]["dice"][i];


            scores[playerI + 1] = newScore;
            dice_values[playerI + 1] = newDice;
        }
        room_screen.gameObject.SetActive(false);
        in_game_screen.gameObject.SetActive(true);

        UpdateScores();
        UpdateDice();
    }

    void OnRollUpdate(JsonData data)
    {
        var id = player_id_dictionary[(string)data["player"]];
        int[] newDice = new int[data["dice"].Count];
        for (int i = 0; i < newDice.Length; i++)
            newDice[i] = (int)data["dice"][i];
        
        dice_values[id] = newDice;
        if ((string)data["player"] == username.text)
            notRolled = false;
        UpdateDice();
    }

    void Finish_Update(JsonData data)
    {
        int id = player_id_dictionary[(string)data["player"]];
        upper_sums[id] = (int)data["uppersum"];
        grand_totals[id] = (int)data["total"];

        UpdateScores();
    }

    void Game_Finished(JsonData data)
    {
        winning_text.text = (string)data["winner"] + " has won the game!";
        winning_text.gameObject.SetActive(true);

        for (int i = 0; i < other_player_cards.Length; i++)
        {
            other_player_cards[i].SetActive(false);
        }
    }

    private void HandleIncomingMessage(byte[] bytes)
    {
        var message = System.Text.Encoding.UTF8.GetString(bytes);

        //var message = "{\"type\":\"warning\",\"data\":[1,2,3]}";

        JsonData json = JsonMapper.ToObject(message);

        switch ((string)json["type"])
        {
            case "room_update":
                OnRoomUpdate(json["data"]);
                break;
            case "roll_update":
                OnRollUpdate(json["data"]);
                break;
            case "score_update":
                HandleScoreInput(json["data"]);
                break;
            case "room_list":
                UpdateRoomList(json["data"]);
                break;
            case "finish_update":
                Finish_Update(json["data"]);
                break;
            case "end_game":
                Game_Finished(json["data"]);
                break;
            case "warning":
                DisplayWarning((string)json["data"]);
                break;
            default:
                break;
        }
    }

    string GetWarningGerman(string identifier) => identifier switch
    {
        "maximum_rolls" => "Alle Würfe verbraucht",
        "not_rolled" => "Nicht gewürfelt",
        "row_in_use" => "Reihe bereits belegt",
        "invalid_username" => "Nutzername nicht verfügbar",
        "room_full" => "Raum voll",
        "room_name_in_use" => "Raumname bereits verwendet",
        "username_already_in_room" => "Nutzername bereits verwendet",
        "room_does_not_exists" => "Raum existiert nicht",
        "invalid_room_name" => "Raumname nicht verfügbar",
        "already_in_room" => "Bereits in einem Raum",
        "finished" => "Du bist schon fertig",
        _ => ""
    };

    string GetWarningEnglish(string identifier) => identifier switch
    {
        "maximum_rolls" => "All rolls used",
        "not_rolled" => "You haven't rolled yet",
        "row_in_use" => "Row already used",
        "invalid_username" => "Username not available",
        "room_full" => "Room full",
        "room_name_in_use" => "Roomname already in use",
        "username_already_in_room" => "Username already in room",
        "room_does_not_exists" => "Room doesn't exist",
        "invalid_room_name" => "Roomname not available",
        "already_in_room" => "Already in room",
        "finished" => "You're already finished",
        _ => ""
    };


    async public void Connect()
    {
        websocket = new WebSocket(websocketAdress);

        websocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");
            RequestRoomList();

        };

        websocket.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
            SetupVars();
            websocket = null;
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
            SetupVars();
            websocket = null;
        };

        websocket.OnMessage += HandleIncomingMessage;

        // Keep sending messages at every 0.3s
        // InvokeRepeating("SendWebSocketMessage", 0.0f, 0.3f);

        // waiting for messages
        await websocket.Connect();
    }

    void Update()
    {
        #if !UNITY_WEBGL || UNITY_EDITOR
            if (websocket != null) 
               websocket.DispatchMessageQueue();
        #endif
    }


    private async void OnApplicationQuit()
    {
        if (websocket != null)
            await websocket.Close();
    }

    async public void SendMessage(string type, JsonData data)
    {
        JsonData newData = new JsonData();
        newData["type"] = type;
        newData["data"] = data;
        if (websocket != null)
            await websocket.SendText(JsonMapper.ToJson(newData));
    }
}
