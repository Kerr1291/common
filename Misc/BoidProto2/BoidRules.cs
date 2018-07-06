using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

using nv;

public class BoidRules : MonoBehaviour
{
    [System.Serializable]
    public class Rule
    {
        [Range(0.0f,10.0f)]
        public float weight = 0.0f;

        public Vector3 vector = Vector3.zero;
    }

    [System.Serializable]
    public class CrowdingRule : Rule
    {
        [Range(0.0f,100.0f)]
        public float minBoidDistance = 1.0f;
        [Range(0.0f,100.0f)]
        public float minWallDistance = 1.0f;

        [Range(0.0f,10.0f)]
        public float boidAwayWeight = 0.0f;
        public Vector3 boidAwayVector = Vector3.zero;

        public AnimationCurve boidAwayDistancePower;

        [Range(0.0f,10.0f)]
        public float wallAwayWeight = 0.0f;
        public Vector3 wallAwayVector = Vector3.zero;

        public AnimationCurve wallAwayDistancePower;
    }

    public Rule centerOfMassRule;
    public CrowdingRule crowdingRule;
    public Rule flockingRule;
    public Rule forceRule;
    public Rule focusLocationRule;

    //boid rule 1 - move towards center of 'mass'
    public Vector3 Rule1Vector( CubeBoid boid )
    {
        return GetBoidRuleVector( centerOfMassRule, boid );
    }

    //boid rule 2 - move away from things
    public Vector3 Rule2Vector( CubeBoid boid )
    {
        GetBasicRuleVector( crowdingRule, boid );
        GetBoidRuleVector( crowdingRule, boid );

        crowdingRule.vector = crowdingRule.boidAwayVector + crowdingRule.wallAwayVector;

        return crowdingRule.vector * crowdingRule.weight;
    }

    public Vector3 Rule3Vector( CubeBoid boid )
    {
        return GetBoidRuleVector( flockingRule, boid );
    }

    public Vector3 Rule4Vector( CubeBoid boid )
    {
        return forceRule.vector * forceRule.weight;
    }

    public Vector3 Rule5Vector( CubeBoid boid )
    {
        return ( focusLocationRule.vector - boid._transform.localPosition) * focusLocationRule.weight;
    }

    Vector3 GetBasicRuleVector( Rule rule, CubeBoid boid )
    {
        if( !boid.rulesDirty )
            return rule.vector;

        UpdateRules( boid, boid.Swarm );
        boid.rulesDirty = false;

        return rule.vector;
    }

    Vector3 GetBoidRuleVector( Rule rule, CubeBoid boid )
    {
        if( !boid.rule3Dirty )
            return rule.vector;

        UpdateRules( boid, boid.CubeSwarm );
        boid.rule3Dirty = false;

        return rule.vector;
    }

    void UpdateRules( CubeBoid boid, List<Collider> swarm )
    {
        if( swarm.Count <= 0 )
            return;

        Vector3 pos = boid._transform.localPosition;
        Dev.VectorXZ( ref pos );

        //rule vectors
        Vector3 awayDir = Vector3.zero;
        float c = 0;
        for( int i = 0; i < swarm.Count; ++i )
        {
            if( swarm[ i ] == null )
                continue;

            Vector3 boundsPoint = swarm[i].ClosestPointOnBounds(pos);
            Dev.VectorXZ(ref boundsPoint);

            //calculate rule 2
            Vector3 diff = boundsPoint - pos;
            float dist = diff.magnitude;

            if( dist < crowdingRule.minWallDistance )
            {
                float normalizedAwayPower = Mathf.Clamp01(dist / crowdingRule.minWallDistance);

                float awayPower = crowdingRule.wallAwayDistancePower.Evaluate(normalizedAwayPower);

                awayDir = awayDir - diff * awayPower;
            }

            c += 1f;
        }

        //finalize rule 2
        crowdingRule.wallAwayVector = awayDir * crowdingRule.wallAwayWeight;
    }

    void UpdateRules( CubeBoid boid, List<CubeBoid> swarm )
    {
        if( swarm.Count <= 0 )
        {
            //finalize rule 1
            //centerOfMassRule.vector = nv.Dev.VectorXZ( -boid._transform.localPosition ).normalized;
            //finalize rule 3
            flockingRule.vector = Vector3.zero;
            return;
        }

        Vector3 pos = boid._transform.localPosition;
        Dev.VectorXZ( ref pos );

        Vector3 awayDir = Vector3.zero;
        Vector3 positionSum = Vector3.zero;
        Vector3 velocitySum = Vector3.zero;
        float vc = 0;
        float pc = 0;
        for( int i = 0; i < swarm.Count; ++i )
        {
            if( swarm[ i ] == null )
                continue;

            //Vector3 boundsPoint = swarm[i].boidCollider.ClosestPointOnBounds(pos);
            Vector3 boundsPoint = swarm[i]._transform.localPosition;
            Dev.VectorXZ( ref boundsPoint );

            //calculate rule 2
            Vector3 diff = boundsPoint - pos;
            float dist = diff.magnitude;
            
            if( dist < crowdingRule.minBoidDistance )
            {
                float normalizedAwayPower = Mathf.Clamp01(dist / crowdingRule.minBoidDistance);

                float awayPower = crowdingRule.boidAwayDistancePower.Evaluate(normalizedAwayPower);

                awayDir = awayDir - diff * awayPower;
            }

            if( dist < FlockRadius(boid) )
            {
                //calculate for rule 1
                positionSum += swarm[ i ]._transform.localPosition;
                pc += 1f;

                //calculate for rule 3
                Rigidbody body = swarm[ i ].body;
                if( body != null )
                {
                    velocitySum += body.velocity;
                    vc += 1f;
                }
            }

        }

        Dev.VectorXZ( ref positionSum );
        Dev.VectorXZ( ref velocitySum );

        Vector3 boidPos = pos;

        //finalize rule 1
        centerOfMassRule.vector = ( ( positionSum / pc ) - boidPos ) * centerOfMassRule.weight;

        //finalize rule 2
        crowdingRule.boidAwayVector = awayDir * crowdingRule.boidAwayWeight;

        //finalize rule 3
        flockingRule.vector = ( ( velocitySum / vc ) ) * flockingRule.weight;
    }

    public Vector3 GetMovementVector( CubeBoid boid )
    {
        Vector3 result = Rule1Vector( boid ) + Rule2Vector( boid ) + Rule3Vector( boid ) + Rule4Vector( boid ) + Rule5Vector( boid );
        centerOfMassRule.vector = Vector3.zero;
        crowdingRule.vector = Vector3.zero;
        flockingRule.vector = Vector3.zero;
        crowdingRule.boidAwayVector = Vector3.zero;
        crowdingRule.wallAwayVector = Vector3.zero;
        return result;
    }

    public float FlockRadius( CubeBoid boid )
    {
        return boid.swarmDetectionArea.radius * 1.5f;
    }
}