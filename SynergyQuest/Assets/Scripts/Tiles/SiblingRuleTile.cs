using System.Collections.Generic;
using UnityEngine.Tilemaps;

namespace UnityEngine
{
    /**
     * Variation of a rule tile which accepts certain other rule tiles as neighbors.
     * These other tiles must be added to the "siblings" property of this tile.
     *
     * It can be used to either place decorations / tiles with variations within a cluster of rule tiles or to decide
     * certain configuration manually where the rules are not enough.
     *
     * It is based on the code snippet which the github users "johnsoncodehk", "ElnuDev" and "gamercoon" developed in
     * this issue: https://github.com/Unity-Technologies/2d-extras/issues/67
     */
    [CreateAssetMenu(fileName = "New Sibling Rule Tile", menuName = "Tiles/Sibling Rule Tile")]
    public class SiblingRuleTile : RuleTile
    {
        public List<TileBase> siblings;

        public override bool RuleMatch(int neighbor, TileBase other)
        {
            switch(neighbor)
            {
                case TilingRule.Neighbor.This:
                    return (siblings.Contains(other) 
                        || base.RuleMatch(neighbor, other));
                case TilingRule.Neighbor.NotThis:
                    return (!siblings.Contains(other)
                        && base.RuleMatch(neighbor, other));
            }
            return base.RuleMatch(neighbor, other);
        }
    }
}
