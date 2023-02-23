
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CardManager : MonoBehaviour
{
    
    public Card CardPrefab;
    public Transform CardSpawnPosition; 
    public Vector2 StartPosition = new Vector2(-2.15f, 3.62f); //where the cards will spawn first -2.15f, 3.62f

    // different states of game
    public enum GameState
    {
        NoAction,
        MovingOnPositions,
        DeletingCards,
        FlipBack,
        Checking,
        GameEnd
    };

    // current state of the puzzle (cards specifically)
    public enum PuzzleState 
    { 
        PuzzleRotating,
        CanRotate // idle state

    };

    // state of revealed cards
    public enum RevealedState
    {
        NoRevealed,
        OneRevealed,
        TwoRevealed

    };

    // Hide variables in inspector to avoid manipulation 
    [HideInInspector]
    public GameState CurrentGameState;
    [HideInInspector]
    public PuzzleState CurrentPuzzleState;
    [HideInInspector]
    public RevealedState PuzzleRevealedNumber;

    
    [HideInInspector]
    public List<Card> CardList;

    private Vector2 _offset = new Vector2(1.5f, 1.52f); // how much the cards will shift from start position 1.5f, 1.52f
    private Vector2 _offsetFor15Pairs = new Vector2(1.08f, 1.22f);
    private Vector2 _offsetFor20Pairs = new Vector2(1.08f, 1.0f);
    private Vector3 _newScaleDown = new Vector3(0.9f, 0.9f, 0.001f);


    private List<Material> _materialList = new List<Material>();
    private List<string> _texturePathList = new List<string>();
    private Material _firstMaterial;
    private string _firstTexturePath;

    private int _firstRevealedCard;
    private int _secondRevealedCard;
    private int _revealedCardNumber = 0;

    // Start is called before the first frame update
    void Start()
    {
        CurrentGameState = GameState.NoAction;
        CurrentPuzzleState = PuzzleState.CanRotate;
        PuzzleRevealedNumber = RevealedState.NoRevealed;
        _revealedCardNumber = 0;
        _firstRevealedCard = -1;
        _secondRevealedCard = -1;

        LoadMaterials();

        // load easy level with 10 pairs
        if (GameSettings.Instance.GetPairNumber() == GameSettings.EPairNumber.E10Pairs)
        {
            CurrentGameState = GameState.MovingOnPositions;
            SpawnCardMesh(4, 5, StartPosition, _offset, false);  // specify grid size r*c, start, offset, scaling
            MoveCard(4, 5, StartPosition, _offset); // perform movement 
        }

        // load medium level with 15 pairs
        else if (GameSettings.Instance.GetPairNumber() == GameSettings.EPairNumber.E15Pairs)
        {
            CurrentGameState = GameState.MovingOnPositions;
            SpawnCardMesh(5, 6, StartPosition, _offset, false);  
            MoveCard(5, 6, StartPosition, _offsetFor15Pairs); 
        }

        // load medium level with 15 pairs
        else if (GameSettings.Instance.GetPairNumber() == GameSettings.EPairNumber.E20Pairs)
        {
            CurrentGameState = GameState.MovingOnPositions;
            SpawnCardMesh(5, 8, StartPosition, _offset, true); 
            MoveCard(5, 8, StartPosition, _offsetFor20Pairs);  
        }

    }

    // Load the card texture materials using directory of stored location
    private void LoadMaterials()
    {
        var materialFilePath = GameSettings.Instance.GetMaterialDirectoryName();
        var textureFilePath = GameSettings.Instance.GetPuzzleCategoryTextureDirectoryName();
        var pairNumber = (int)GameSettings.Instance.GetPairNumber();
        const string matBaseName = "Pic";  //base name for all card textures that are stored
        var firstMaterialName = "Back";

        // Loop through all selected pairs and select each material to add to list of materials
        for (var index = 1; index <= pairNumber; index++)
        {
            var currentFilePath = materialFilePath + matBaseName + index; // mat for x amount of cards using base name + index 
            Material mat = Resources.Load(currentFilePath, typeof(Material)) as Material;
            _materialList.Add(mat);

            //get texture names for the second material(front side of card), add  to list of textures
            var currentTextureFilePath = textureFilePath + matBaseName + index;
            _texturePathList.Add(currentTextureFilePath);


        }

        _firstTexturePath = textureFilePath + firstMaterialName;
        _firstMaterial = Resources.Load(materialFilePath + firstMaterialName, typeof(Material)) as Material;
    }


    // Update is called once per frame
    void Update()
    {
        
    }



    // add each card into prefab list  ,grid of positioned cards
    private void SpawnCardMesh(int rows, int columns, Vector2 Pos, Vector2 offset, bool scaleDown)
    {
      
        for (int col = 0; col < columns; col++)
        {
            for(int row = 0; row < rows; row++)
            {
                var tempCard = (Card)Instantiate(CardPrefab, CardSpawnPosition.position, CardPrefab.transform.rotation);

                if(scaleDown)
                {
                    tempCard.transform.localScale = _newScaleDown;
                }

                //tempCard.transform.localScale = new Vector3(250f, 250f, 0f); // Set scale to (250, 250, 0)
                tempCard.name = tempCard.name + 'c' + col + 'r' + row;
                CardList.Add(tempCard);
            }
        }

        ApplyTextures();
    }




    // randomise texures applied to cards, allows cards to be in different locations each time game is played
    public void ApplyTextures()
    {
        var rndMatIndex = Random.Range(0, _materialList.Count);
        var AppliedTimes = new int[_materialList.Count];   // array to apply each texture twice to create pairs

        for (int i = 0; i < _materialList.Count; i++)
        {
            AppliedTimes[i] = 0;
        }


        foreach (var o in CardList)
        {
            var randPrevious = rndMatIndex;
            var counter = 0;
            var forceMat = false;

            // only allow 2 of the same card on grid
            while (AppliedTimes[rndMatIndex] >= 2 || ((randPrevious == rndMatIndex) && !forceMat))
            {
                // check if there are any materials left that haven't been applied to two cards
                bool hasAvailableMaterials = false;
                for (int j = 0; j < _materialList.Count; j++)
                {
                    if (AppliedTimes[j] < 2)
                    {
                        hasAvailableMaterials = true;
                        break;
                    }
                }

                if (!hasAvailableMaterials)
                {
                    Debug.LogError("Not enough available materials to create a pair for a card");
                    return;
                }

                rndMatIndex = Random.Range(0, _materialList.Count);
                counter++;

                // After 100 counts to apply specific materials, check to apply first available material if it has not been applied yet 
                if (counter > 100)
                {
                    for (var j = 0; j < _materialList.Count; j++)
                    {
                        if (AppliedTimes[j] < 2)
                        {
                            rndMatIndex = j;
                            forceMat = true;
                        }
                    }

                    if (forceMat == false)
                        return;
                }
            }


            // set and apply first material, randomise applying second material from material list
            o.SetFirstMaterial(_firstMaterial, _firstTexturePath);
            o.ApplyFirstMaterial();
            o.SetSecondMaterial(_materialList[rndMatIndex], _texturePathList[rndMatIndex]);

            //o.ApplySecondMaterial(); //TEST

            AppliedTimes[rndMatIndex] += 1;
            forceMat = false;


        }

    }

    // make cards moveable to a target position
    private IEnumerator MoveToPosition(Vector3 target, Card obj)
    {
        var randomDis = 7;  //  how far each card will move

        while (obj.transform.position != target) // whilst current pos not equal to target pos
        {
            obj.transform.position = Vector3.MoveTowards(obj.transform.position, target, randomDis * Time.deltaTime);
            yield return 0;
        }
    }

    // Move each card to specified final position on game start by using a coroutine
    private void MoveCard(int rows, int columns, Vector2 pos, Vector2 offset)
    {
        var index = 0;
        for(var col=0; col < columns; col++)
        {
            for(int row = 0; row < rows; row++)
            {
                //create targets by multiplying offset by row number of prefab clones + x direction,  y direction subtract multiplication of offset by col number 
                var targetPosition = new Vector3((pos.x + (offset.x * row)), (pos.y - (offset.y * col)), 0.0f);  
                StartCoroutine(MoveToPosition(targetPosition, CardList[index])); // move cards and run the function over several frames
                index++;
            }
        }
    }

    

}
