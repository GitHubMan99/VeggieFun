using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    private Material _firstMaterial;
    private Material _secondMaterial;

    private Quaternion _currentRotation; //represent rotation

    [HideInInspector]
    public bool Revealed = false;
    private CardManager _cardManager;



    void Start()
    {
        Revealed = false;
        _cardManager = GameObject.Find("[CardManager]").GetComponent<CardManager>();
        _currentRotation = gameObject.transform.rotation;
    }


    void Update()
    {

    }

    private void OnMouseDown()
    {
        StartCoroutine(LoopRotation(45, false));
    }

    // cards can switch back to first material if clicked on again
    /*public void FlipBack()
    {
        if(gameObject.activeSelf)
        {
            CardManager.CurrentPuzzleState = CardManager.PuzzleState.PuzzleRotating;
            Revealed = false;
            StartCoroutine(LoopRotation(45, false));

        }
    }*/

    // Rotate cards to flip from first to second material
    IEnumerator LoopRotation(float angle, bool FirstMat)
    {
        var rot = 0F;
        const float dir = 1f;
        const float rotSpeed = 180.0f;
        const float rotSpeed1 = 90.0f;
        var startAngle = angle;
        var assigned = false;

        if(FirstMat)
        {
            while(rot < angle)
            {
                var step = Time.deltaTime * rotSpeed1;
                gameObject.GetComponent<Transform>().Rotate(new Vector3(0, 2, 0) * step * dir);
                if (rot >= (startAngle - 2) && assigned == false)
                {
                    ApplyFirstMaterial();
                    assigned = true;
                }

                rot += (1 * step * dir);
                yield return null;
            }
        }
        else
        {
            while(angle > 0 )
            {
                float step = Time.deltaTime * rotSpeed;
                gameObject.GetComponent <Transform>().Rotate(new Vector3(0, 2, 0) * step * dir);
                angle -= (1 * step * dir);
                yield return null;
            }
        }

        gameObject.GetComponent<Transform>().rotation = _currentRotation;

        if(!FirstMat)
        {
            Revealed = true;
            ApplySecondMaterial();
        }
        else
        {
           /* _cardManager.PuzzleRevealedNumber = CardManager.RevealedState.NoRevealed;
            _cardManager.CurrentPuzzleState = _cardManager.PuzzleState.CanRotate;*/
        }
    }

    //load textures from resources folder and assign texture as first material
    public void SetFirstMaterial(Material mat, string texturePath)
    {
        _firstMaterial = mat;
        _firstMaterial.mainTexture = Resources.Load(texturePath, typeof(Texture2D)) as Texture2D;
        _firstMaterial.mainTexture = Resources.Load<Texture2D>(texturePath);

    }

    //load textures from resources folder and assign texture as second material
    public void SetSecondMaterial(Material mat, string texturePath)
    {
        _secondMaterial = mat;
        _secondMaterial.mainTexture = Resources.Load(texturePath, typeof(Texture2D)) as Texture2D;
        _secondMaterial.mainTexture = Resources.Load<Texture2D>(texturePath);
    }


    public void ApplyFirstMaterial()
    {
        gameObject.GetComponent<Renderer>().material = _firstMaterial;
    }

    public void ApplySecondMaterial()
    {
        gameObject.GetComponent<Renderer>().material = _secondMaterial;
    }

}
