using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Node _nodePrefab;
    [SerializeField] private Block _blockPrefab;
    [SerializeField] private GameObject _boardPrefab;

    [SerializeField] private Animator _winScreen;
    [SerializeField] private Animator _loseScreen;

    private List<Node> _nodes;
    private List<Block> _blocks;

    private GameState _gameState;

    [SerializeField] private int _numberOfRows = 4;
    [SerializeField] private int _numberOfColumns = 4;

    [SerializeField] private List<BlockType> _blockTypes;

    public BlockType GetBlockTypeByValue(int value) => _blockTypes.First(b => b.Value == value);

    public bool IsInputEnabled => _gameState == GameState.WaitingInput;

    private int _blocksToSpawn = 2;
    private float _shiftTime = 0.3f;

    private int _winCondition = 2048;

    void ChangeState(GameState gameState)
    {
        _gameState = gameState;

        switch (gameState)
        {
            case GameState.GeneratingGrid:
                GenerateGrid();
                break;
            case GameState.SpawningBlocks:
                SpawnBlocks(_blocksToSpawn-- == 2 ? 2 : 1);
                break;
            case GameState.WaitingInput:
                break;
            case GameState.ShiftingBlocks:
                break;
            case GameState.Win:
                WonGame();
                break;
            case GameState.Lose:
                LoseGame();
                break;
            default:
                break;
        }
    }

    void Start()
    {
        _nodes = new List<Node>();
        _blocks = new List<Block>();

        GenerateGrid();
    }

    void GenerateGrid()
    {
        for (int x = 0; x < _numberOfRows; x++)
        {
            for (int y = 0; y < _numberOfColumns; y++)
            {
                var node = Instantiate(_nodePrefab, new Vector2(x, y), Quaternion.identity);
                _nodes.Add(node);
            }
        }

        var center = new Vector2((_numberOfRows / 2) - 0.5f, (_numberOfColumns / 2) - 0.5f);

        Instantiate(_boardPrefab, center, Quaternion.identity);
        Camera.main.transform.position = new Vector3(center.x, center.y, -10);

        ChangeState(GameState.SpawningBlocks);
    }

    public void Restart()
    {
        Debug.Log("Button Pressed");
        SceneManager.LoadScene(0);
    }

    void SpawnBlocks(int numberOfBlockToSpawn)
    {
        var freeNodes = _nodes.Where(n => n.BlockOnThisNode == null).OrderBy(b => UnityEngine.Random.value).Take(numberOfBlockToSpawn).ToList();

        foreach (Node node in freeNodes)
        {
            SpawnBlock(UnityEngine.Random.value < 0.9 ? 2 : 4, node);
        }

        if (_blocks.FirstOrDefault(b => b.Value == _winCondition))
        {
            ChangeState(GameState.Win);
            return;
        }
        if (AreThereAnyLegalMovesLeft())
            ChangeState(GameState.WaitingInput);
        else
            ChangeState(GameState.Lose);


    }

    void WonGame()
    {
        _winScreen.gameObject.SetActive(true);
        _winScreen.Play("FadeInCanvas");
    }

    void LoseGame()
    {
        _loseScreen.gameObject.SetActive(true);
        _loseScreen.Play("FadeInCanvas");
    }

    void SpawnBlock(int value, Node node)
    {
        var block = Instantiate(_blockPrefab, node.Pos, Quaternion.identity);
        block.Init(GetBlockTypeByValue(value));
        block.SetBlock(node);
        _blocks.Add(block);
    }

    public void Shift(Vector2 direction)
    {
        if (!isThisALegalMove(direction))
            return;

        ChangeState(GameState.ShiftingBlocks);

        var orderedBlocks = _blocks.OrderBy(n => n.Pos.x).ThenBy(n => n.Pos.y).ToList();

        if (direction == Vector2.right || direction == Vector2.up)
            orderedBlocks.Reverse();

        foreach (Block block in orderedBlocks)
        {
            var nextNode = block.OccupiedNode;
            do
            {
                block.SetBlock(nextNode);

                var possibleNode = GetNodeAt(nextNode.Pos + direction);

                if (possibleNode != null)
                {
                    if (possibleNode.BlockOnThisNode != null && possibleNode.BlockOnThisNode.CanBeMerged(block.Value))
                    {
                        block.MergeBlock(possibleNode.BlockOnThisNode);
                    }
                    else if (possibleNode.BlockOnThisNode == null)
                    {
                        nextNode = possibleNode;
                    }
                }
            }
            while (nextNode != block.OccupiedNode);

        }

        var sequence = DOTween.Sequence();

        foreach (Block block in orderedBlocks)
        {
            var movePoint = block.MergingBlock != null ? block.MergingBlock.OccupiedNode.Pos : block.OccupiedNode.Pos;
            sequence.Insert(0, block.transform.DOMove(movePoint, _shiftTime));
        }

        sequence.OnComplete(() =>
           {
               Debug.Log(orderedBlocks.Where(b => b.MergingBlock != null).Count());
               foreach (Block block in orderedBlocks.Where(b => b.MergingBlock != null))
               {

                   Debug.Log(block.Value + "- " + block.Pos);
                   MergeBlocks(block.MergingBlock, block);
               }

               ChangeState(GameState.SpawningBlocks);
           });
    }

    void MergeBlocks(Block baseBlock, Block secondBlock)
    {
        SpawnBlock(baseBlock.Value * 2, baseBlock.OccupiedNode);

        _blocks.Remove(baseBlock);
        _blocks.Remove(secondBlock);

        Destroy(baseBlock.gameObject);
        Destroy(secondBlock.gameObject);
    }

    public Node GetNodeAt(Vector2 Pos) => _nodes.FirstOrDefault(n => n.Pos == Pos);

    public bool isThisALegalMove(Vector2 direction)
    {
        foreach (Block block in _blocks)
        {
            var nextNode = block.OccupiedNode;
            do
            {
                var possibleNode = GetNodeAt(nextNode.Pos + direction);

                if (possibleNode != null)
                    if (possibleNode.BlockOnThisNode == null)
                        return true;
                    else if (possibleNode.BlockOnThisNode.CanBeMerged(block.Value))
                        return true;

            } while (nextNode != block.OccupiedNode);

        }
        return false;
    }

    public bool AreThereAnyLegalMovesLeft()
    {
        if (isThisALegalMove(Vector2.left) || isThisALegalMove(Vector2.right) || isThisALegalMove(Vector2.up) || isThisALegalMove(Vector2.down))
            return true;
        else
            return false;
    }

    void Update()
    {
        if (_gameState == GameState.WaitingInput)
        {
            if (Input.GetKey(KeyCode.A) && isThisALegalMove(Vector2.left)) Shift(Vector2.left);
            if (Input.GetKey(KeyCode.W) && isThisALegalMove(Vector2.up)) Shift(Vector2.up);
            if (Input.GetKey(KeyCode.S) && isThisALegalMove(Vector2.down)) Shift(Vector2.down);
            if (Input.GetKey(KeyCode.D) && isThisALegalMove(Vector2.right)) Shift(Vector2.right);
        }
    }
}
[Serializable]
public struct BlockType
{
    public int Value;
    public Color Color;
    public Color TextColor;
}

enum GameState
{
    GeneratingGrid,
    SpawningBlocks,
    WaitingInput,
    ShiftingBlocks,
    Win,
    Lose
}