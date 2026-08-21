using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Cysharp.Threading.Tasks;

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
    [FoldoutGroup("Character Spawn")]
    private List<Characters> characterList = new List<Characters>()
    {
        new Characters() { character = Character.Kael, characterData = null },
        new Characters() { character = Character.Lyra, characterData = null },
    };

    [Header("Current Character Properties")]
    [SerializeField][FoldoutGroup("Character Spawn")] private Characters currentCharacter;
    [SerializeField][FoldoutGroup("Character Spawn")] private CharacterBase currentCharacterBase;
    public CharacterBase CurrentCharacterBase => currentCharacterBase;

    [Header("Selecting Character Properties")]
    [SerializeField][FoldoutGroup("Character Spawn")] private Characters selectingCharacter;


    [Header("Spawn Points")]
    [SerializeField][FoldoutGroup("Character Spawn")] private Vector3 offsetSpawnPoint = new Vector3(0, 0f, 0);
    [SerializeField][FoldoutGroup("Character Spawn")] private Transform spawnPointInLobby;

    [SerializeField][FoldoutGroup("Character ReSpawn")] private Transform reSpawnPoint;
    [SerializeField][FoldoutGroup("Character ReSpawn")] private float radiusFindRespawnPoint = 500f;
    [SerializeField][FoldoutGroup("Character ReSpawn")] private float distanceRespawnPointToSafeZoneEdge = 10f;   //Khoảng cách của điểm respawn đến rìa bo 
    [SerializeField][FoldoutGroup("Character ReSpawn")] private LayerMask respawnPointLayerMask;
    [SerializeField][FoldoutGroup("Character ReSpawn")] private float delayRespawn = 3f;

    #region Init 

    private void Start()
    {
        EventManager.Subscribe(GameEvent.OnPlayerDeath, CheckRespawnCharacter);
    }

    private void OnDestroy()
    {
        EventManager.Unsubscribe(GameEvent.OnPlayerDeath, CheckRespawnCharacter);
    }

    private Characters LoadCharacterSelected()
    {
        Character selected = Character.Kael;
        CharacterUnlockData savedData =
            SaveLoadManager.Instance?.SaveData?.characterData;

        if (savedData != null &&
            savedData.selectedCharacter == CharacterType.Lyra &&
            savedData.lyraUnlocked)
        {
            selected = Character.Lyra;
        }

        Characters result =
            characterList.Find(item => item.character == selected);

        return result ??
               characterList.Find(item => item.character == Character.Kael) ??
               characterList[0];
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
    [FoldoutGroup("Character Spawn")]
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
    [FoldoutGroup("Character Spawn")]
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
    [FoldoutGroup("Character Spawn")]
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

    #region Respawn

    public async void CheckRespawnCharacter(object data = null)
    {
        await UniTask.Delay((int)(delayRespawn * 1000));
        UIManager.Instance?.ChangeMenu(MenuType.YouDiedRespawnMenu, true); // Hiện menu respawn
        await UniTask.Delay(2000);

        MapType mapType = GameManager.Instance != null ? GameManager.Instance.MapType : MapType.None;

        if (mapType == MapType.Tutorial)
        {
            TutorialSkipHandler handler =
                FindFirstObjectByType<TutorialSkipHandler>();

            handler?.SkipOrCompleteTutorial();
            return;
        }

        if (mapType == MapType.Boss)
        {
            RunGameplayController.Instance?.TriggerEndRun(false);
            return;
        }

        if (currentCharacterBase == null)
        {
            Debug.LogError("Chưa spawn nhân vật hiện tại");
            return;
        }

        reSpawnPoint = null;
        if (GameManager.Instance.MapType == MapType.Run) // Nếu map run thì respawn
        {

            // Respawn lúc chạy bo
            reSpawnPoint = FindRespawnPoint();

            if (reSpawnPoint == null)
                return;

            CharacterController characterController = currentCharacterBase.CharacterMovement?.CC;
            currentCharacterBase.CharacterMovement?.Stop();

            if (characterController != null)
                characterController.enabled = false;

            currentCharacterBase.transform.position = reSpawnPoint.position + offsetSpawnPoint;
            currentCharacterBase.gameObject.SetActive(true);
            currentCharacterBase.ResetCharacter();
            currentCharacterBase.CharacterLevel?.DecreaseLevel();

            if (characterController != null)
                characterController.enabled = true;

            Physics.SyncTransforms();
        }
    }

    /// <summary>
    /// Tìm vị trí respawn gần nhất dựa trên currentMapType
    /// </summary>
    private Transform FindRespawnPoint()
    {
        Collider[] colliders = Physics.OverlapSphere(currentCharacterBase.transform.position, radiusFindRespawnPoint, respawnPointLayerMask);

        //Tìm điểm respawn gần nhất dựa trên khoảng cách từ vị trí hiện tại của nhân vật
        Transform nearestRespawnPoint = null;
        float nearestDistance = Mathf.Infinity;
        List<Transform> validRespawnPoints = new List<Transform>();

        //SECTION - TH1: Ưu tiên: Tìm điểm respawn gần nhất trong vòng bo và có cách rìa bo khoảng cách distanceRespawnPointToSafeZoneEdge
        foreach (Collider collider in colliders)
        {
            // Tính khoảng cách từ player đến điểm respawn
            float distance = Vector3.Distance(currentCharacterBase.transform.position, collider.transform.position);
            if (distance < nearestDistance) // So sánh khoảng cách đó với khoảng cách gần nhất đã tìm được
            {
                validRespawnPoints.Add(collider.transform); // Thêm vào danh sách tạm thời
                if (SafeZoneManager.Instance.
                                    CheckObjectInSafeZone(collider.transform,
                                    distanceRespawnPointToSafeZoneEdge)) // Kiểm tra điểm respawn có nằm trong vòng bo hay không và có cách rìa bo khoảng cách distanceRespawnPointToSafeZoneEdge hay không
                {
                    nearestDistance = distance;
                    nearestRespawnPoint = collider.transform;
                }
            }
        }

        //SECTION - TH2: Nếu không tìm thấy điểm respawn trong vòng bo, có cách rìa 1 khoảng, thì tìm điểm gần nhất trong bo trong danh sách tạm thời
        if (nearestRespawnPoint == null && validRespawnPoints.Count > 0)
        {
            // Nếu không tìm thấy điểm respawn hợp lệ trong vòng bo, chọn điểm respawn gần nhất từ danh sách tạm thời
            nearestRespawnPoint = validRespawnPoints[0];
            float minDistance = Vector3.Distance(currentCharacterBase.transform.position, nearestRespawnPoint.position);
            foreach (Transform respawnPoint in validRespawnPoints)
            {
                float distance = Vector3.Distance(currentCharacterBase.transform.position, respawnPoint.position);
                if (distance < minDistance)
                {
                    if (SafeZoneManager.Instance.
                                        CheckObjectInSafeZone(respawnPoint)) // Kiểm tra điểm respawn có nằm trong vòng bo hay không
                    {
                        minDistance = distance;
                        nearestRespawnPoint = respawnPoint;
                    }
                }
            }
        }

        //SECTION - TH3: Nếu vẫn không tìm thấy điểm respawn trong vòng bo, thì tìm điểm gần nhất trong danh sách tạm thời mà không cần kiểm tra vòng bo
        if (nearestRespawnPoint == null && validRespawnPoints.Count > 0)
        {
            // Nếu vẫn không tìm thấy điểm respawn hợp lệ trong vòng bo, chọn điểm respawn gần nhất từ danh sách tạm thời mà không cần kiểm tra vòng bo
            nearestRespawnPoint = validRespawnPoints[0];
            float minDistance = Vector3.Distance(currentCharacterBase.transform.position, nearestRespawnPoint.position);
            foreach (Transform respawnPoint in validRespawnPoints)
            {
                float distance = Vector3.Distance(currentCharacterBase.transform.position, respawnPoint.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestRespawnPoint = respawnPoint;
                }
            }
        }

        if (nearestRespawnPoint == null)
        {
            Debug.LogWarning("Không tìm thấy điểm respawn gần nhân vật");
        }
        return nearestRespawnPoint;
    }
    #endregion
}
