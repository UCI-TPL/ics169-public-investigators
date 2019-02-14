﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementParticle : MonoBehaviour {

    int particleID;
    string particleName;
    public float particleLife;
    string particleOwner;
    Rigidbody2D body;
    LevelManager levelManager;


    public void InitElementParticle(ParticleManager pm, int id, float pLife, float pForce, Transform t, string pO, LevelManager lm, string pName, float rotationRange, float lifeRange, float forceRange)
    {
        tag = id + "-" + pName;
        particleID = id;
        particleName = pName;
        float lR = Random.Range(-lifeRange, lifeRange);
        particleLife = pLife + lR;
        if (particleLife < 0) particleLife = 0.25f;

        gameObject.transform.rotation = t.rotation;
        gameObject.transform.Rotate(new Vector3(0f, 0f, Random.Range(-rotationRange, rotationRange)));

        body = GetComponent<Rigidbody2D>();
        float fR = Random.Range(-forceRange, forceRange);
        body.AddForce(transform.right * (pForce + fR));

        particleOwner = pO;
        levelManager = lm;

        StartCoroutine("DestroyParticle");
    }


    public int GetParticleID()
    {
        return particleID;
    }


    public string GetParticleName()
    {
        return particleName;
    }


    public IEnumerator DestroyParticle()
    {
        yield return new WaitForSeconds(particleLife);
        Destroy(gameObject);
    }


    public string GetOwner()
    {
        return particleOwner;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Walls" || collision.tag == "Untagged" || collision.tag[0] == 'R' || collision.tag == "TEST")
        {
            return;
        }

        ElementParticle ep = collision.GetComponent<ElementParticle>();
        if (ep != null && ep.GetOwner() == GetOwner())
        {
            return;
        }


        PlayerInfo pi = null;
        string elemName = this.tag.Split('-')[1];
        if (collision.tag == "Player")
        {
            pi = collision.gameObject.GetComponent<PlayerInfo>();

        }

        if (pi == null) return;
        //ElementObject element = collision.gameObject.GetComponent<ElementObject>();
        //Debug.Log("PI: " + pi);
        if (elemName == "Fire" || elemName == "Water" || elemName == "Steam") // checks if element is colliding with player and does damage if its enemy
        {
            //particleOwner = "Player" + GetComponentInParent<PlayerInfo>().playerNum;
            if (particleOwner == ("Player" + pi.playerNum.ToString()))
            {
                return;
            }
            if (pi.health <= 0 || pi.isRespawning)
            {
                pi.health = 0;
                pi.isRespawning = true;
            }

            if (pi == null) return;
            //Debug.Log(particleOwner+ "Health: " + pi.health);

            PlayerCollisionModel.CollisionResult result = levelManager.playerCollisionModel.HandleCollision(pi.health, elemName);
            //Debug.Log(result);
            pi.health = result.health;
            //pi.gameObject.GetComponent<PlayerController>().HandleEffects(result.effect, collision.gameObject.transform);
            //Debug.Log("New PlayerHealth: " + pi.health);
        }
    }
}