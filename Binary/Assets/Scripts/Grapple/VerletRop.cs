using UnityEngine;
using System;
using System.Collections.Generic;

public class VerletRop : MonoBehaviour
{
    [Serializable]
    public class RopePoint
    {
        public Vector2 Position;
        public Vector2 OldPosition;
        public bool IsFixed;

        public RopePoint(Vector2 pos, bool fix = false)
        {
            Position = pos;
            OldPosition = pos;
            IsFixed = fix;
        }
    }

    [Header("Configuration de la Corde")]
    [SerializeField] private int _SegmentCount = 15;
    [SerializeField] private float _SegmentLength = 0.4f;
    [SerializeField] private float _Gravity = 9.8f;
    [SerializeField] private float _Damping = 0.98f;
    [SerializeField] private int _ConstraintIterations = 3;

    [Header("Animations")]
    [SerializeField] private float _LaunchSpeed = 20f;
    [SerializeField] private float _LaunchDuration = 0.3f;
    [SerializeField] private AnimationCurve _LaunchCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float _DestructionDuration = 0.2f;
    [SerializeField] private AnimationCurve _DestructionCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("Rendu")]
    [SerializeField] private LineRenderer LineRendererComponent;
    [SerializeField] private float _RopeWidth = 0.08f;
    [SerializeField] private Material RopeMaterial;

    [Header("Particules")]
    [SerializeField] private ParticleSystem _DestructionParticles;
    [SerializeField] private int _ParticleCount = 15;
    [SerializeField] private float _ParticleSpeed = 8f;
    [SerializeField] private Color _ParticleColor = Color.blue;

    private List<RopePoint> _ropePoints = new List<RopePoint>();
    private bool _isActive = false;
    private float _actualSegmentLength;

    private bool _isLaunching = false;
    private bool _isDestroying = false;
    private Vector2 _launchTarget;
    private Vector2 _launchStart;


    //TODO setup line renderer in inspector and particle systm

    /// <summary>
    /// Créer une corde entre le joueur et un point d'ancrage
    /// </summary>
    public void CreateRope(Vector2 playerPosition, Vector2 anchorPoint)
    {
        _launchStart = playerPosition;
        _launchTarget = anchorPoint;

        //Todo animation

        _ropePoints.Clear();

        float totalDistance = Vector2.Distance(playerPosition, anchorPoint);
        _actualSegmentLength = totalDistance / _SegmentCount;

        Vector2 direction = (anchorPoint - playerPosition).normalized;

        // Créer les points de la corde
        for (int i = 0; i <= _SegmentCount; i++)
        {
            Vector2 pointPosition = playerPosition + direction * (_actualSegmentLength * i);
            bool isFixedPoint = (i == _SegmentCount); // Le dernier point est l'ancrage

            _ropePoints.Add(new RopePoint(pointPosition, isFixedPoint));
        }

        if (LineRendererComponent != null)
        {
            LineRendererComponent.positionCount = _ropePoints.Count;
            LineRendererComponent.enabled = true;
        }

        _isActive = true;
    }

    /// <summary>
    /// Détruire la corde
    /// </summary>
    public void DestroyRope()
    {
        _ropePoints.Clear();
        _isActive = false;

        if (LineRendererComponent != null)
            LineRendererComponent.enabled = false;
    }

    /// <summary>
    /// Mettre à jour la position du joueur (premier point)
    /// </summary>
    public void UpdatePlayerPosition(Vector2 playerPos)
    {
        if (_ropePoints.Count > 0)
        {
            _ropePoints[0].Position = playerPos;
            _ropePoints[0].OldPosition = playerPos;
        }
    }

    /// <summary>
    /// Obtenir la position calculée du joueur par la simulation
    /// </summary>
    public Vector2 GetSimulatedPlayerPosition()
    {
        if (_ropePoints.Count > 0)
            return _ropePoints[0].Position;
        return Vector2.zero;
    }

    /// <summary>
    /// Obtenir la direction de tension de la corde
    /// </summary>
    public Vector2 GetTensionDirection()
    {
        if (_ropePoints.Count >= 2)
        {
            return (_ropePoints[1].Position - _ropePoints[0].Position).normalized;
        }
        return Vector2.zero;
    }

    /// <summary>
    /// Obtenir la longueur totale de la corde
    /// </summary>
    public float GetRopeLength()
    {
        return _actualSegmentLength * _SegmentCount;
    }

    public bool IsActive() => _isActive && _ropePoints.Count > 0;

    void FixedUpdate()
    {
        if (!_isActive || _ropePoints.Count == 0) return;

        SimulateVerlet();
        ApplyConstraints();
        UpdateLineRenderer();
    }

    private void SimulateVerlet()
    {
        float deltaTime = Time.fixedDeltaTime;

        for (int i = 1; i < _ropePoints.Count - 1; i++) // Ignorer le premier (joueur) et dernier (ancrage)
        {
            Vector2 temp = _ropePoints[i].Position;
            Vector2 acceleration = Vector2.down * _Gravity;

            _ropePoints[i].Position = _ropePoints[i].Position +
                                   (_ropePoints[i].Position - _ropePoints[i].OldPosition) * _Damping +
                                   acceleration * deltaTime * deltaTime;

            _ropePoints[i].OldPosition = temp;
        }
    }

    private void ApplyConstraints()
    {
        for (int iteration = 0; iteration < _ConstraintIterations; iteration++)
        {
            for (int i = 0; i < _ropePoints.Count - 1; i++)
            {
                ApplyDistanceConstraint(i, i + 1);
            }
        }
    }

    private void ApplyDistanceConstraint(int indexA, int indexB)
    {
        RopePoint pointA = _ropePoints[indexA];
        RopePoint pointB = _ropePoints[indexB];

        Vector2 delta = pointB.Position - pointA.Position;
        float distance = delta.magnitude;

        if (distance > 0)
        {
            float difference = (_actualSegmentLength - distance) / distance;
            Vector2 translate = delta * difference * 0.5f;

            if (!pointA.IsFixed) pointA.Position -= translate;
            if (!pointB.IsFixed) pointB.Position += translate;
        }
    }

    private void UpdateLineRenderer()
    {
        if (LineRendererComponent == null) return;

        Vector3[] positions = new Vector3[_ropePoints.Count];
        for (int i = 0; i < _ropePoints.Count; i++)
        {
            positions[i] = new Vector3(_ropePoints[i].Position.x, _ropePoints[i].Position.y, 0);
        }

        LineRendererComponent.SetPositions(positions);
    }
}
