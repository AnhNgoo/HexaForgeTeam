using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DuskBlade.Tests
{
    public static class TestPrefabFinder
    {
        private static readonly string[] PreferredNameTokens =
        {
            "Player",
            "Kael",
            "Enemy",
            "Monster",
            "HUD",
            "UI",
            "Camera"
        };

        private static readonly string[] PlayerComponentNames =
        {
            "CharacterBase",
            "CharacterMovement",
            "CharacterCombat",
            "CharacterSkill",
            "CharacterLockTarget",
            "Kael"
        };

        private static readonly string[] EnemyComponentNames =
        {
            "EnemyBase",
            "EnemyHealth",
            "EnemyCombat",
            "EnemyDamageReceiver",
            "EnemyDetection",
            "EnemyLocomotion"
        };

        public static GameObject FindPlayerPrefab()
        {
            GameObject characterPrefab = FindCharacterPrefab("Kael");
            if (characterPrefab != null) return characterPrefab;

            characterPrefab = FindCharacterPrefab("Lyra");
            return characterPrefab ?? FindBestPrefab(PlayerComponentNames, new[] { "Player", "Kael", "Lyra" });
        }

        public static GameObject FindEnemyPrefab()
        {
            return FindBestPrefab(EnemyComponentNames, new[] { "Enemy", "Monster" });
        }

        public static GameObject FindHudOrUiPrefab()
        {
            return FindBestPrefab(new string[0], new[] { "HUD", "UI" });
        }

        public static GameObject FindCameraPrefab()
        {
            return FindBestPrefab(new string[0], new[] { "Camera" });
        }

        public static GameObject FindBestKnownPrefab()
        {
            return FindBestPrefab(Concat(PlayerComponentNames, EnemyComponentNames), PreferredNameTokens);
        }

        public static GameObject FindBestPrefab(string[] componentNames, string[] nameTokens)
        {
#if UNITY_EDITOR
            string[] guids = AssetDatabase.FindAssets("t:Prefab");
            GameObject bestPrefab = null;
            int bestScore = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    continue;
                }

                int score = ScorePrefab(prefab, componentNames, nameTokens);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestPrefab = prefab;
                }
            }

            return bestScore > 0 ? bestPrefab : null;
#else
            return null;
#endif
        }

        public static GameObject FindPrefabByNameToken(string nameToken)
        {
            if (string.IsNullOrWhiteSpace(nameToken))
            {
                return null;
            }

            return FindBestPrefab(new string[0], new[] { nameToken });
        }

        public static GameObject FindPrefabWithComponent(string componentName)
        {
            if (string.IsNullOrWhiteSpace(componentName))
            {
                return null;
            }

            return FindBestPrefab(new[] { componentName }, new string[0]);
        }

            private static GameObject FindCharacterPrefab(string characterName)
            {
        #if UNITY_EDITOR
                string path = "Assets/_Data/Resources/Prefabs/Characters/" + characterName + ".prefab";
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) return null;

                Component characterBase = TestReflectionHelper.FindComponentByClassName(prefab, "CharacterBase");
                Component movement = TestReflectionHelper.FindComponentByClassName(prefab, "CharacterMovement");
                return characterBase != null && movement != null ? prefab : null;
        #else
                return Resources.Load<GameObject>("Prefabs/Characters/" + characterName);
        #endif
            }

        private static int ScorePrefab(GameObject prefab, string[] componentNames, string[] nameTokens)
        {
            int score = 0;

            if (nameTokens != null)
            {
                foreach (string token in nameTokens)
                {
                    if (!string.IsNullOrWhiteSpace(token) &&
                        prefab.name.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        score += 1;
                    }
                }
            }

            if (componentNames != null && componentNames.Length > 0)
            {
                var expectedComponents = new HashSet<string>(componentNames);
                Component[] components = prefab.GetComponentsInChildren<Component>(true);
                foreach (Component component in components)
                {
                    if (component == null)
                    {
                        continue;
                    }

                    Type type = component.GetType();
                    if (expectedComponents.Contains(type.Name) || expectedComponents.Contains(type.FullName))
                    {
                        score += 10;
                    }
                }
            }

            return score;
        }

        private static string[] Concat(string[] first, string[] second)
        {
            var result = new string[first.Length + second.Length];
            Array.Copy(first, 0, result, 0, first.Length);
            Array.Copy(second, 0, result, first.Length, second.Length);
            return result;
        }
    }
}
