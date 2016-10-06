﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour {

	public float speed;						//Movement speed
	public float enemy_speed;				//Enemies movement speed
	public float bulletSpeed;				//Players bullet movement speed
	public GameObject bullet;				//Instance of player's bullet. Will be used to create new bullets 
	public GameObject enemyPlane;           //Instance of enemy planes. Will be used to create new enemies 
	public float reloadTime;				//Tells the time between bullets are fired
	public float HP;						//Player's health points
	public Text gameOverText;				//UI text which appears when player dies

	private float reloadTimeRemaining;		//How much time is left before next shooting
	private bool readyToShoot;				//Tells if player is ready to shoot new bullet

	private int hitDamage;                  //How many HPs player is going to lose when hitted by enemy planes or enemy bullets

	private EnemyController enemyController;    //EnemyController script

	// Use this for initialization
	void Start () {
		gameOverText.text = "";
		hitDamage = -5;
		reloadTimeRemaining = reloadTime;
		readyToShoot = true;
		Timer();
	}
	
	// Update is called once per frame
	void Update () {
		//reset velocity to zero
		GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);

		//Movement: up, down, right and left. Shoot when moving
		//Play area: x: -23 to +23 and y: -10 to +10
		//if player is out of play area, moving further is not allowed
		if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)){
			if (transform.position.y <= 10)
				GetComponent<Rigidbody2D>().velocity = new Vector2(GetComponent<Rigidbody2D>().velocity.x, speed);
			Shoot();
		}
		else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)){
			if (transform.position.y >= -10)
				GetComponent<Rigidbody2D>().velocity = new Vector2(GetComponent<Rigidbody2D>().velocity.x, -speed);
			Shoot();
		}
		if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)){
			if (transform.position.x <= 23)
				GetComponent<Rigidbody2D>().velocity = new Vector2(speed, GetComponent<Rigidbody2D>().velocity.y);
			Shoot();
		}
		else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)){
			if (transform.position.x >= -23)
				GetComponent<Rigidbody2D>().velocity = new Vector2(-speed, GetComponent<Rigidbody2D>().velocity.y);
			Shoot();
		}

	}

	//Function for shooting
	void Shoot()
	{
		//Able to shoot == not reloading?
		if (!readyToShoot)
		{
			reloadTimeRemaining -= Time.deltaTime;
			if (reloadTimeRemaining <= 0)
			{
				readyToShoot = true;
				reloadTimeRemaining = reloadTime;
			}
		}

		//Has the turrets been reloaded now? If yes -> shoot
		if (readyToShoot)
		{
			//LeftTurret
			GameObject newBullet1 = Instantiate(bullet);
			SetupBullet(newBullet1, "LeftTurret");

			//RightTurret
			GameObject newBullet2 = Instantiate(bullet);
			SetupBullet(newBullet2, "RightTurret");

			readyToShoot = false;
		}

	}

	//Setup the position and velocity of the new bullet
	void SetupBullet(GameObject aBullet, string turret)
	{
		aBullet.transform.position =
			new Vector3(transform.Find(turret).transform.position.x,
			transform.Find(turret).transform.position.y,
			transform.Find(turret).transform.position.z);
		aBullet.SetActive(true);
		aBullet.GetComponent<Rigidbody2D>().velocity = new Vector2(bulletSpeed, 0);

	}

	//The timer of the game. Defines when enemies appears.
	void Timer()
	{
		StartCoroutine(Wait(1));
	}

	//Defines how much we are waiting before enemies appears
	IEnumerator Wait(float time)
	{
		/*yield return new WaitForSeconds(time);
		createNewEnemy(new Vector2(27, 6), -Mathf.PI);
		createNewEnemy(new Vector2(27, 1), -Mathf.PI);
		createNewEnemy(new Vector2(27, -4), -Mathf.PI);

		yield return new WaitForSeconds(8);
		createNewEnemy(new Vector2(27, -2), -Mathf.PI);
		createNewEnemy(new Vector2(27, -7), -Mathf.PI);
		createNewEnemy(new Vector2(27, -11), -Mathf.PI);

		yield return new WaitForSeconds(3);
		createNewEnemy(new Vector2(4, -14), Mathf.PI * 3 / 4);
		createNewEnemy(new Vector2(10, -14), Mathf.PI * 3 / 4);
		createNewEnemy(new Vector2(16, -14), Mathf.PI * 3 / 4);*/

		yield return new WaitForSeconds(1);
		
		//Spline for new enemy
		List<Vector2> list = new List<Vector2>();
		list.Add(new Vector2(30, 0));
		list.Add(new Vector2(7, -20));
		list.Add(new Vector2(-11, -30));
		list.Add(new Vector2(-30, -50));

		createNewEnemy(list[0], -Mathf.PI, list);

	}
	
	//Ends the game with notifications
	void gameOver()
	{
		gameOverText.text = "Game Over!";

		StartCoroutine(WaitForRestart());

	}

	IEnumerator WaitForRestart()
	{
		yield return new WaitForSeconds(3);
		SceneManager.LoadScene("Main");
	}

	//Collision handler
	void OnTriggerEnter2D(Collider2D other)
	{
		//Player hits enemy bullets
		if (other.gameObject.tag == "Bullet")
		{
			changeHealth(hitDamage);
			Destroy(other.gameObject);
		}
		else if (other.gameObject.tag == "Enemy")
		{
			changeHealth(hitDamage);
		}
	}

	//Creates a new enemy to the given postition and directions
	//Rotation is given in radians. -PI == -180 ==> moving from right to left
	void createNewEnemy(Vector2 pos, float rot, List<Vector2> list = null)
	{
		GameObject newEnemy = Instantiate(enemyPlane);
		newEnemy.transform.position = pos;
		newEnemy.SetActive(true);

		newEnemy.GetComponent<Rigidbody2D>().velocity =
			new Vector2(Mathf.Cos(rot) * enemy_speed, Mathf.Sin(rot) * enemy_speed);

		//rot and Matf.PI are summed so we can get right rotation angle
		//left == 0, down == 90, right == 180 and up == 270 (in degrees)
		rot += Mathf.PI;
		newEnemy.transform.Rotate(new Vector3(0, 0, Mathf.Rad2Deg * rot), Space.Self);

		if (list != null)
		{
			newEnemy.GetComponent<EnemyController>().setupSpline(list);
		}
	}

	void changeHealth(int amount)
	{
		HP += amount;
		if(HP <= 0)
		{
			gameOver();
		}
	}
}