using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public enum Character
{
    None = 0,
    Kael = 1,
    Lyra = 2,
}

[System.Serializable]
public class Characters
{
    public Character character;
    public CharacterData characterData;
}
public class PlayerManager : Singleton<PlayerManager>
{
    [SerializeField]
    private List<Characters> characterList = new List<Characters>()
    {
        new Characters() { character = Character.Kael, characterData = null },
        new Characters() { character = Character.Lyra, characterData = null },
    };

    [Header("Current Character Properties")]
    [SerializeField] private Characters currentCharacter;
    [SerializeField] private CharacterBase currentCharacterBase;

    [Header("Selecting Character Properties")]
    [SerializeField] private Characters selectingCharacter;


    [Header("Spawn Points")]
    [SerializeField] private Vector3 offsetSpawnPoint = new Vector3(0, 0f, 0);
    [SerializeField] private Transform spawnPointInLobby;

    #region Init 

    // private void Start()
    // {
    //     SpawnCharacterInLobby();
    // }

    private Characters LoadCharacterSelected()
    {
        string selectedCharacter = PlayerPrefs.GetString("SelectedCharacter", Character.Kael.ToString());
        Character character = (Character)System.Enum.Parse(typeof(Character), selectedCharacter);
        Characters _character = characterList.Find(c => c.character == character);
        if (_character != null)
        {
            return _character;
        }
        else
        {
            return characterList[0];
        }
    }

    private void SaveCharacterSelected()
    {
        PlayerPrefs.SetString("SelectedCharacter", currentCharacter.character.ToString());
        PlayerPrefs.Save();
    }
    #endregion

    #region Spawn and Change Characters

    /// <summary>
    /// Chọn nhân vật khác
    /// </summary>
    /// <param name="character"></param>
    [Button("Select Character Test")]
    public CharacterData SelectCharacter(Character character)
    {
        Characters _character = characterList.Find(c => c.character == character);
        if (_character != null)
        {
            this.selectingCharacter = _character;
            return _character.characterData;
        }
        return null;
    }

    /// <summary>
    /// Kiểm tra xem nhân vật vừa chọn có phải là nhân vật hiện tại hay không
    /// Sử dụng để ẩn nút confirm chọn nhân vật khi nhân vật vừa chọn là nhân vật hiện tại
    /// </summary>
    /// <returns></returns>
    public bool IsSelectedCharacter()
    {
        if (selectingCharacter == null || currentCharacter == null)
            return false;

        return selectingCharacter.character == currentCharacter.character;
    }
    /// <summary>
    /// Spawn nhân vật đã thay đổi ở vị trí nhân vật đang dứng
    /// </summary>
    [Button("Spawn Character Selected Test")]
    public void SpawnCharacterInCurrentPosition()
    {
        if (selectingCharacter.character == Character.None)
        {
            Debug.LogError("Chưa chọn nhân vật để spawn");
            return;
        }

        if (selectingCharacter == null)
        {
            Debug.LogError("Chưa chọn nhân vật để spawn");
            return;
        }
        if (currentCharacter.character == selectingCharacter.character)
        {
            Debug.LogWarning("Nhân vật hiện tại đã là nhân vật được chọn");
            return;
        }
        if (currentCharacterBase == null)
        {
            Debug.LogError("Chưa spawn nhân vật hiện tại");
            return;
        }

        SpawnCharacter(selectingCharacter, currentCharacterBase.transform);
        SaveCharacterSelected();
    }

    /// <summary>
    /// Spawn nhân vật hiện tại tại vị trí spawnPointInLobby
    /// </summary>
    [Button("Spawn Character In Lobby Test")]
    public void SpawnCharacterInLobby()
    {
        Characters character = LoadCharacterSelected();

        if (spawnPointInLobby == null)
            FindSpawnPointInLobby();

        if (character == null)
        {
            Debug.LogError("Character is null");
            return;
        }

        SpawnCharacter(character, spawnPointInLobby);
        SaveCharacterSelected();
    }

    private void SpawnCharacter(Characters character, Transform spawnPoint)
    {
        if (currentCharacterBase != null)
        {
            ObjectPooling.Instance.ReturnToPool(currentCharacter.characterData.characterPoolType, currentCharacterBase.gameObject);
            currentCharacterBase = null;
        }
        GameObject characterObject = ObjectPooling.Instance.SpawnFromPool(character.characterData.characterPoolType,
                                                                                spawnPoint.position + offsetSpawnPoint,
                                                                                Quaternion.identity);
        ObjectPooling.Instance.SpawnFromPool(PoolType.SpawnCharacterEffect, spawnPoint.position + new Vector3(0, -1f, 0), Quaternion.identity);

        currentCharacterBase = characterObject?.GetComponent<CharacterBase>();

        CharacterData characterData = Instantiate(character.characterData);

        currentCharacterBase?.Init(characterData);

        currentCharacter.character = character.character;
        currentCharacter.characterData = characterData;
    }

    #endregion

    #region Find Spawn Position 

    public void FindSpawnPointInLobby()
    {
        spawnPointInLobby = GameObject.FindGameObjectWithTag("SpawnPointCharacterInLobby").transform;
        if (spawnPointInLobby == null)
            Debug.LogError("Không tìm thấy SpawnPointCharacterInLobby trong scene");
    }
    #endregion
}
