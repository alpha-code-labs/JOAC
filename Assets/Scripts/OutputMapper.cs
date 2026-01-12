using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutputMapper : MonoBehaviour
{
    Dictionary<string, string> shotsDict { get; } = new Dictionary<string, string>
    {
        { "Backfoot Defense" , "BACKFOOT_DEFENSE"},
        { "Block" , "BACKFOOT_DEFENCE"},
        { "Cover Drive", "COVER_DRIVE"},
        { "Cut Shot" , "CUT_SHOT"},
        { "Forward Defense", "FORWARD_DEFENSE"},
        { "Leg Drive", "LEG_DRIVE"},
        { "Leg Glance", "LEG_GLANCE"},
        { "Pull Shot", "PULL_SHOT"},
        { "Square Drive", "SQUARE_DRIVE"},
        { "Straight Drive", "STRAIGHT_DRIVE"},
        { "Straight Drive off the back foot", "STRAIGHT_DRIVE_BACKFOOT"}
    };

    Dictionary<string, string> ballBehaviourDict { get; } = new Dictionary<string, string>
    {
        { "Ball travels behind the batsman with a deflection", "DEFLECT"},
        { "Ball travels behind the batsman with no connection", "MISS"},
        { "Ball crashes into the stumps", "STUMPS"},
        { "Ball moves straight down the ground", "GROUNDED"},
        { "Batsman misses the ball and ball hits the pads", "LBW"},
        { "Ball moves through the covers in a downward trajectory through the off side", "DWT_COVER_OFF"},
        { "Ball moves through the covers in a upward trajectory through the off side", "UWT_COVER_OFF"},
        { "Ball moves square of the wicket in a downward trajectory through the off side", "DWT_SQUARE_OFF"},
        { "Ball moves square of the wicket in a upward trajectory through the off side", "UWT_SQUARE_OFF"},
        { "Ball moves through the covers in a downward trajectory through the leg side", "DWT_COVER_LEG"},
        { "Ball moves through the covers in a upward trajectory through the leg side", "UWT_COVER_LEG"},
        { "Ball moves square of the wicket in a downward trajectory through the leg side", "DWT_SQUARE_LEG"},
        { "Ball moves square of the wicket in a upward trajectory through the leg side", "UWT_SQUARE_LEG"},
        { "Ball moves down the ground in a downward trajectory straight down", "DWT_STRAIGHT"},
        { "Ball moves down the ground in a upward trajectory straight down", "UWT_STRAIGHT"}
    };

    public (string, string) GetOutcomeFbxName(string inputBatDirection, string oc1, string oc2, string oc3, string oc4) //output (batting prefab name, fielder prefab name)
    {

        oc1 = oc1.Trim();
        oc2 = oc2.Trim();
        oc3 = oc3.Trim();
        oc4 = oc4.Trim();
        inputBatDirection = inputBatDirection.Trim();

        string batsmanFbxPrefix = null;
        string batsmanFbxSuffix = null;
        string fielderFbxName = null;

        if (shotsDict.TryGetValue(oc1, out string value1))
        {
            batsmanFbxPrefix = value1;
        }
        else
        {
            return ("", "");
        }

        if (ballBehaviourDict.TryGetValue(oc2, out string value2))
        {
            batsmanFbxSuffix = value2;
        }
        else
        {
            return ("", "");
        }

        switch (oc3)
        {
            case "Not Applicable":
                {
                    switch (oc4)
                    {
                        case "Wicketkeeper catches the ball":
                            fielderFbxName = "WK_CATCHES";
                            break;
                        case "Bowler picks the ball up":
                            fielderFbxName = "PICKED_BY_BALLER";
                            break;
                        case "LBW Animation":
                            fielderFbxName = "LBW_ANIM";
                            break;
                        case "Wide Animation":
                            fielderFbxName = "WIDE_ANIM";
                            break;
                        case "Not Applicable":
                            fielderFbxName = null;
                            break;
                        default:
                            fielderFbxName = "";
                            break;
                    }
                    break;
                }

            case "Ball crosses the boundary line at speed":
                {
                    if (oc2.Contains("upward"))
                    {
                        switch (inputBatDirection)
                        {
                            case "Square of the Wicket Off Side":
                                fielderFbxName = "CROSSES_BOUNDARY_SQUARE_OFF";
                                break;
                            case "Through the Covers Off Side":
                                fielderFbxName = "CROSSES_BOUONDARY_COVER_OFF";
                                break;
                            case "Straight Down the Ground":
                                fielderFbxName = "CROSSES_BOUONDARY_STRAIGHT";
                                break;
                            case "Through the Covers Leg Side":
                                fielderFbxName = "CROSSES_BOUONDARY_COVER_LEG";
                                break;
                            case "Square of the Wicket Leg Side":
                                fielderFbxName = "CROSSES_BOUONDARY_SQUARE_LEG";
                                break;
                            default:
                                fielderFbxName = "";
                                break;
                        }
                    }
                    else
                    {
                        switch (inputBatDirection)
                        {
                            case "Square of the Wicket Off Side":
                                fielderFbxName = "CROSSES_BOUNDARY_AIR_SQUARE_OFF";
                                break;
                            case "Through the Covers Off Side":
                                fielderFbxName = "CROSSES_BOUONDARY_AIR_COVER_OFF";
                                break;
                            case "Straight Down the Ground":
                                fielderFbxName = "CROSSES_BOUONDARY_AIR_STRAIGHT";
                                break;
                            case "Through the Covers Leg Side":
                                fielderFbxName = "CROSSES_BOUONDARY_AIR_COVER_LEG";
                                break;
                            case "Square of the Wicket Leg Side":
                                fielderFbxName = "CROSSES_BOUONDARY_AIR_SQUARE_LEG";
                                break;
                            default:
                                fielderFbxName = "";
                                break;
                        }
                    }
                    break;
                }

            case "Ball crosses the boundary line at speed in the air":
                {
                    if (oc2.Contains("upward"))
                    {
                        switch (inputBatDirection)
                        {
                            case "Square of the Wicket Off Side":
                                fielderFbxName = "CROSSES_BOUNDARY_SQUARE_OFF";
                                break;
                            case "Through the Covers Off Side":
                                fielderFbxName = "CROSSES_BOUONDARY_COVER_OFF";
                                break;
                            case "Straight Down the Ground":
                                fielderFbxName = "CROSSES_BOUONDARY_STRAIGHT";
                                break;
                            case "Through the Covers Leg Side":
                                fielderFbxName = "CROSSES_BOUONDARY_COVER_LEG";
                                break;
                            case "Square of the Wicket Leg Side":
                                fielderFbxName = "CROSSES_BOUONDARY_SQUARE_LEG";
                                break;
                            default:
                                fielderFbxName = "";
                                break;
                        }
                    }
                    else
                    {
                        switch (inputBatDirection)
                        {
                            case "Square of the Wicket Off Side":
                                fielderFbxName = "CROSSES_BOUNDARY_AIR_SQUARE_OFF";
                                break;
                            case "Through the Covers Off Side":
                                fielderFbxName = "CROSSES_BOUONDARY_AIR_COVER_OFF";
                                break;
                            case "Straight Down the Ground":
                                fielderFbxName = "CROSSES_BOUONDARY_AIR_STRAIGHT";
                                break;
                            case "Through the Covers Leg Side":
                                fielderFbxName = "CROSSES_BOUONDARY_AIR_COVER_LEG";
                                break;
                            case "Square of the Wicket Leg Side":
                                fielderFbxName = "CROSSES_BOUONDARY_AIR_SQUARE_LEG";
                                break;
                            default:
                                fielderFbxName = "";
                                break;
                        }
                    }
                    break;
                }

            case "Ball slows half way between the 30 yard circle and boundary line":
                {
                    switch (inputBatDirection)
                    {
                        case "Square of the Wicket Off Side":
                            fielderFbxName = "SLOWS_HALFWAY_30_SQUARE_OFF";
                            break;
                        case "Through the Covers Off Side":
                            fielderFbxName = "SLOWS_HALFWAY_30_COVERS_OFF";
                            break;
                        case "Straight Down the Ground":
                            fielderFbxName = "SLOWS_HALFWAY_30_STRAIGHT";
                            break;
                        case "Through the Covers Leg Side":
                            fielderFbxName = "SLOWS_HALFWAY_30_COVERS_LEG";
                            break;
                        case "Square of the Wicket Leg Side":
                            fielderFbxName = "SLOWS_HALFWAY_30_SQUARE_LEG";
                            break;
                        default:
                            fielderFbxName = "";
                            break;
                    }
                    break;
                }

            case "Ball slows near the 30 yard circle":
                {
                    switch (inputBatDirection)
                    {
                        case "Square of the Wicket Off Side":
                            fielderFbxName = "SLOWS_NEAR_30_SQUARE_OFF";
                            break;
                        case "Through the Covers Off Side":
                            fielderFbxName = "SLOWS_NEAR_30_COVERS_OFF";
                            break;
                        case "Straight Down the Ground":
                            fielderFbxName = "SLOWS_NEAR_30_STRAIGHT";
                            break;
                        case "Through the Covers Leg Side":
                            fielderFbxName = "SLOWS_NEAR_30_COVERS_LEG";
                            break;
                        case "Square of the Wicket Leg Side":
                            fielderFbxName = "SLOWS_NEAR_30_SQUARE_LEG";
                            break;
                        default:
                            fielderFbxName = "";
                            break;
                    }
                    break;
                }

            case "Ball slows very close to the boundary line":
                {
                    switch (inputBatDirection)
                    {
                        case "Square of the Wicket Off Side":
                            fielderFbxName = "SLOWS_NEAR_BOUNDARY_SQUARE_OFF";
                            break;
                        case "Through the Covers Off Side":
                            fielderFbxName = "SLOWS_NEAR_BOUNDARY_COVERS_OFF";
                            break;
                        case "Straight Down the Ground":
                            fielderFbxName = "SLOWS_NEAR_BOUNDARY_STRAIGHT";
                            break;
                        case "Through the Covers Leg Side":
                            fielderFbxName = "SLOWS_NEAR_BOUNDARY_COVERS_LEG";
                            break;
                        case "Square of the Wicket Leg Side":
                            fielderFbxName = "SLOWS_NEAR_BOUNDARY_SQUARE_LEG";
                            break;
                        default:
                            fielderFbxName = "";
                            break;
                    }
                    break;
                }

            default:
                fielderFbxName = "";
                break;
        }

        if (fielderFbxName != "" && fielderFbxName != null)
            return (batsmanFbxPrefix + "_" + batsmanFbxSuffix, fielderFbxName);

        else return (null, null);
    }
}
