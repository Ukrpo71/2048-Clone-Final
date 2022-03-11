using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Block : MonoBehaviour
{
    public int Value;
    public Node OccupiedNode;

    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private TextMeshPro _text;

    public Block MergingBlock;
    public bool isMergingBaseBlock;

    public bool CanBeMerged(int value) => MergingBlock == null && !isMergingBaseBlock && Value == value;

    public Vector2 Pos => transform.position;
    public void Init(BlockType blockType)
    {
        Value = blockType.Value;
        _spriteRenderer.color = blockType.Color;
        _text.text = Value.ToString();
        _text.color = blockType.TextColor;
    }

    public void SetBlock(Node node)
    {
        if (OccupiedNode != null) 
            OccupiedNode.BlockOnThisNode = null;

        OccupiedNode = node;
        node.BlockOnThisNode = this;

    }

    public void MergeBlock(Block baseBlock)
    {
        MergingBlock = baseBlock;
        baseBlock.isMergingBaseBlock = this;

        OccupiedNode.BlockOnThisNode = null;
    }
}
