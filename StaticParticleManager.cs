using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class StaticParticleManager : MonoBehaviour {
    public Camera m_Camera;
    public TrailParticle particle;
    private Transform m_CameraPos;
    private Vector3 camPosition;
    private Vector3 upVec;
    private List<TrailParticle> particles;

	// Use this for initialization
	void Start () {
	    if (!m_Camera) {
            m_Camera = Camera.main;
        }
	}
	
	// Update is called once per frame
	void Update () {
	    camPosition = m_Camera.transform.rotation * Vector3.back;
        upVec = m_Camera.transform.rotation * Vector3.up;

        foreach (TrailParticle p in particles) {
            p.transform.LookAt(p.transform.position + camPosition, upVec);
        }
	}

    bool CameraIsDirty() {
        return m_CameraPos == m_Camera.transform;
    }
}
