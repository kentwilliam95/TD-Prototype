using System;using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Unit Data SO", fileName = "UnitData")]
public class UnitDataSO : ScriptableObject
{
    public enum AttackDirection
    {
        Left,
        Top,
        Right,
        Bot,
        TopLeft,
        TopRight,
        BotRight,
        BotLeft,
    }

    public Entity prefab;
    public Sprite _sprite;
    public int _cost;
    
    public float _health;
    public float _speed;

    [Header("Melee Attack")] 
    public AnimationClip _mClip;
    public float _damage;
    public float _duration;

    [Header("Range Attack")] 
    public AnimationClip _rClip;
    public float _rdamage;
    public float _rduration;
    
    public Projectile _projectile;
    public float _projectileSpeed;
    public AttackDirection[] attackDirection;

    public Vector2Int GetAttackDirectionVector2(AttackDirection direction)
    {
        switch (direction)
        {
            case AttackDirection.Top:
                return Vector2Int.up;

            case AttackDirection.Right:
                return Vector2Int.right;

            case AttackDirection.Bot:
                return Vector2Int.down;

            case AttackDirection.Left:
                return Vector2Int.left;

            case AttackDirection.TopRight:
                return new Vector2Int(1, 1);

            case AttackDirection.BotRight:
                return new Vector2Int(1, -1);

            case AttackDirection.BotLeft:
                return new Vector2Int(-1, -1);

            case AttackDirection.TopLeft:
                return new Vector2Int(-1, 1);
        }

        return default;
    }

    public Vector2Int CalculateGroundIndex(AttackDirection direction, Vector2Int index)
    {
        Vector2Int modifier = GetAttackDirectionVector2(direction);
        index += modifier;
        return index;
    }
}