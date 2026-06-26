using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace DuskBlade.Tests
{
    public static class TestEnemySpawnHelper
    {
        public static GameObject SpawnEnemyWithCampLifecycle(
            GameObject prefab,
            Vector3 position,
            string nameSuffix,
            System.Collections.Generic.ICollection<UnityEngine.Object> spawned)
        {
            Assert.IsNotNull(prefab, "Khong tim thay Enemy prefab that trong project.");
            Type campType = FindType("CampSpawner");
            Type spawnNodeType = FindType("SpawnNode");
            Assert.IsNotNull(campType, "Khong tim thay class CampSpawner that trong project.");
            Assert.IsNotNull(spawnNodeType, "Khong tim thay class SpawnNode that trong project.");

            GameObject campObject = new GameObject("Test_CampSpawner");
            spawned?.Add(campObject);
            Component camp = campObject.AddComponent(campType);
            Behaviour campBehaviour = camp as Behaviour;
            if (campBehaviour != null) campBehaviour.enabled = false;

            GameObject spawnPointObject = new GameObject("Test_EnemySpawnPoint");
            spawned?.Add(spawnPointObject);
            spawnPointObject.transform.position = position;
            spawnPointObject.transform.rotation = Quaternion.identity;

            object node = Activator.CreateInstance(spawnNodeType);
            TestReflectionHelper.TrySetValue(node, "spawnPoint", spawnPointObject.transform);
            TestReflectionHelper.TrySetValue(node, "savedHealth", -1f);
            TestReflectionHelper.TrySetValue(node, "isDead", false);
            TestReflectionHelper.TrySetValue(node, "isPatroller", false);

            Type listType = typeof(List<>).MakeGenericType(spawnNodeType);
            IList nodes = (IList)Activator.CreateInstance(listType);
            nodes.Add(node);
            TestReflectionHelper.TrySetValue(camp, "enemiesInCamp", nodes);

            GameObject enemy = UnityEngine.Object.Instantiate(prefab, position, Quaternion.identity);
            enemy.name = prefab.name + nameSuffix;
            spawned?.Add(enemy);

            Component enemyBase = TestReflectionHelper.FindComponentByClassName(enemy, "EnemyBase");
            Assert.IsNotNull(enemyBase, "Enemy prefab that khong co EnemyBase nen khong the init theo CampSpawner.");

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            Transform playerTransform = player != null ? player.transform : null;

            TestReflectionHelper.TrySetValue(node, "spawnedEnemyObject", enemy);
            TestReflectionHelper.TrySetValue(node, "enemyInstance", enemyBase);
            Assert.IsTrue(
                TestReflectionHelper.TryInvokeMethod(enemyBase, "InitFromCamp", camp, node, playerTransform),
                "Khong goi duoc EnemyBase.InitFromCamp(CampSpawner, SpawnNode, PlayerTransform) bang reflection.");

            return enemy;
        }

        private static Type FindType(string typeName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(typeName);
                if (type != null) return type;

                Type[] types;
                try { types = assembly.GetTypes(); }
                catch (ReflectionTypeLoadException exception) { types = exception.Types; }

                foreach (Type candidate in types)
                {
                    if (candidate != null && (candidate.Name == typeName || candidate.FullName == typeName))
                    {
                        return candidate;
                    }
                }
            }

            return null;
        }
    }
}
