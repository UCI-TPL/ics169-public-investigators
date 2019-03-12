﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo : MonoBehaviour
{

    //Level manager should be passed to player when instantiated
    LevelManager levelManager;
    Fairies vacuum;
    BoxCollider2D vacuumArea;
    public ParticleSystem deathParticles;
    public float health;
	public int lives = 1;
    public int playerNum;
    public AudioSource[] audioSources = new AudioSource[4];
	private HealthBar HPbar;
    private bool isRespawning = false;
    private string rearea = "";
    List<ParticleSystem.Particle> enter = new List<ParticleSystem.Particle>();
    List<ParticleSystem.Particle> exit = new List<ParticleSystem.Particle>();
    private string elementOwnerName ="";
    private bool isFlashing = false;
    private bool resetHP = false;
    Color c;
    PlayerController pc;
    GameObject gravePrefab;

    public void InitPlayerInfo(LevelManager lm, int pNum)
    {
        levelManager = lm;
        vacuum = new Fairies(levelManager);
        audioSources[0] = gameObject.AddComponent<AudioSource>();
        audioSources[1] = gameObject.AddComponent<AudioSource>();
        audioSources[2] = gameObject.AddComponent<AudioSource>();
        health = 100.0f;
        playerNum = pNum;
        pc = GetComponent<PlayerController>();
        c = GetComponent<SpriteRenderer>().material.color;

    }


    private void Start()
    {
		HPbar = GetComponent<HealthBar>();
    }


    public Fairies GetVacuum()
    {
        return vacuum;
    }


    public float GetPlayerHealth()
    {
        return health;
    }


    public void RemovePlayerHealth(int hp){
        health -= hp;
        HPbar.hb.SetActive(true);
        if (!resetHP)
        {
            StartCoroutine("StartTimer");
            resetHP = true;
        }
       
    }

    IEnumerator StartTimer()
    {

        yield return new WaitForSeconds(3);
        HPbar.hb.SetActive(false);
        resetHP = false;
    }

    public string GetPlayerName(){
        return "Player" + playerNum.ToString();
    }


    public bool isDead(){
        return health <= 0.0f;
     
    }


    private void Update()
    {
		//Sets Healthbar
        if (health <= 0)
        {
            HPbar.SetSize(0.0f);
        }
        else { HPbar.SetSize(health / 100f); }

        //Check to see if player is dead
        if(isDead() && !isRespawning){
            //Start respawn coroutine
            StartCoroutine("respawn");
        }
    }
    public void SpawnGrave(Vector3 position)
    {
        // add animation stuff here
        GameObject grave = Instantiate(gravePrefab, position, Quaternion.Euler(transform.forward));
    }

    private IEnumerator respawn(){
        isRespawning = true;
		lives += -1;
        //deathParticles = Instantiate(levelManager.particles[3], transform.position, transform.rotation);
        pc.StartDeathAnimation(); // stops player movement
        SpawnGrave(transform.position);
        //levelManager.SpawnParticleEffectAtPosition(transform.position, 3);
        levelManager.soundManager.PlaySoundByName(audioSources[0], "Death", false, 1.0f); // plays death sound
       
        yield return new WaitForSeconds(1.8f);
		Vector3 respawn = new Vector3(Random.Range(0, 40), Random.Range(0, 40), 0); // temp respawn fix
        //respawn = GetVector(Random.Range(1, 4));
        //Can specify respawn location before Coroutine is started and save as a temporary class variable
		switch (rearea) {
		case "1":
			respawn = GetVector(4);
			break;
		case "2":
			respawn = GetVector(3);
			break;
		case "3":
			respawn = GetVector(2);
			break;
		case "4":
			respawn = GetVector(1);
			break;
		}
        transform.position = respawn;
        pc.SetRevive();

        health = 100.0f;
        isRespawning = false;
        levelManager.GetKillCounter().addKill(GetPlayerName(), elementOwnerName);
    

    }

   

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Walls" || collision.tag == "Player" || collision.tag == "Untagged" || collision.tag[0] == 'R' || collision.tag == "TEST")
        {
			rearea = collision.name;
            //levelManager.soundManager.PlaySoundByName(audioSources[1], "Bump");
			return;
		}
        
        ElementObject elementObj = collision.gameObject.GetComponent<ElementObject>();
        ElementParticle elementPart = collision.gameObject.GetComponent<ElementParticle>();

        int elemID = 0;
        string elemName = "";
        
        if (elementObj != null && elementObj.GetOwner() == ("Player" + playerNum.ToString())) return;
        else if (elementObj != null)
        {
            elemID = elementObj.GetID();
            elemName = elementObj.GetName();
            elementOwnerName = elementObj.GetOwner();
            bool isProjectile = elementObj.GetIsProjectile();

            //If it is not a projectile, then there is no interaction (except maybe physics, so we don't care)
            if (!isProjectile)
            {
                return;
            }
        }
        else if (elementPart != null && elementPart.GetOwner() == ("Player" + playerNum.ToString())) return;
        else if (elementPart != null)
        {
            elemID = elementPart.GetParticleID();
            elemName = elementPart.GetParticleName();
            elementOwnerName = elementPart.GetOwner();
        }

        if (elemName == "") return;

        PlayerCollisionModel.CollisionResult result = levelManager.playerCollisionModel.HandleCollision(health, elemName);
        if (health != result.health && !isFlashing)
        {
            StartCoroutine("DamageFlash");
        }
        health = result.health;
        transform.gameObject.GetComponent<PlayerController>().HandleEffects(result.playerEffect, collision.transform);
    }



    private IEnumerator DamageFlash()
    {
        float time = 0;
        isFlashing = true;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        while (time < .05f) // time it takes to stop flashing 
        {
            time += Time.deltaTime;
   
            sr.enabled = (false); 
            yield return new WaitForSeconds(.15f);
            sr.enabled = (true); 
            yield return new WaitForSeconds(.15f);
        }
        isFlashing = false;
        
    }

    private Vector3 GetRandomVector(int x_range, int y_range)
    {
		if (x_range >= 0f && y_range >= 0f)
			return new Vector3(Random.Range(0f,x_range),Random.Range(0f,y_range),-2f);
		else if(x_range < 0f && y_range >= 0f)
			return new Vector3(Random.Range(x_range,0f),Random.Range(0f,y_range),-2f);
		else if(x_range >= 0f && y_range < 0f)
			return new Vector3(Random.Range(0,x_range),Random.Range(y_range,0f),-2f);
		else if(x_range < 0f && y_range < 0f)
			return new Vector3(Random.Range(x_range,0f),Random.Range(y_range,0f),-2f);
		return new Vector3 (0, 0, 0);
	}


	private Vector3 GetVector(int area)
    {
		Vector3 re = new Vector3(0,0,0);
		switch(area){
		case 1:
			re = GetRandomVector(-9,4);
			break;
		case 2:
			re = GetRandomVector(9,4);
			break;
		case 3:
			re = GetRandomVector(-9,-4);
			break;
		case 4:
			re = GetRandomVector(9,-4);
			break;
		}
		return re;
	}
}
