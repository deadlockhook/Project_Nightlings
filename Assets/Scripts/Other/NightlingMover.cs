using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NightlingMover : MonoBehaviour
{
	public GameObject monster;
	public Transform startPos;
	public Transform targetPos;
	public float moveDuration = 5f;

	public float minSpawnInterval = 35f;
	public float maxSpawnInterval = 45f;

	void Start()
	{
		if (monster != null)
		{
			monster.SetActive(false);
		}

		StartCoroutine(SpawnAndMoveMonster());
	}

	IEnumerator SpawnAndMoveMonster()
	{
		while (true)
		{
			float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
			yield return new WaitForSeconds(waitTime);

			monster.transform.position = startPos.position;
			monster.SetActive(true);

			float elapsed = 0f;
			while (elapsed < moveDuration)
			{
				monster.transform.position = Vector3.Lerp(startPos.position, targetPos.position, elapsed / moveDuration);
				elapsed += Time.deltaTime;
				yield return null;
			}

			monster.transform.position = targetPos.position;

			monster.transform.position = startPos.position;
			monster.SetActive(false);
		}
	}
}
