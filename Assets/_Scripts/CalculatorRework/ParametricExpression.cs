﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParametricExpression : MonoBehaviour
{
    Expressions.ExpressionType type;
    Expressions expressionsClass;
    ExpressionSet expSet;
    List<Transform> expressionsList;
    List<Transform> variableClumps;
    Dictionary<string, Transform> variables;
    Dictionary<string, Transform> hiddenVariables;
    List<Transform> emptyList;
    Transform separator;
    Scroll scroll;
    bool initialized = false;
    float xPos = 1.2f;
    bool deleteVar = false;
    bool destroyCalled = false;
    bool isActive = true;
    List<string> varsToDelete;

    void Awake()
    {
        if (initialized) return;
        type = Expressions.ExpressionType.Paramet;
        expressionsClass = Expressions._instance;
        expSet = new ExpressionSet();
        expressionsList = new List<Transform>();
        variableClumps = new List<Transform>();
        variables = new Dictionary<string, Transform>();
        hiddenVariables = new Dictionary<string, Transform>();
        emptyList = new List<Transform>();
        varsToDelete = new List<string>();

        scroll = expressionsClass.getScroll("param");
        initialized = true;
    }

    public void Initialize()
    {
        if (!initialized)
        {
            type = Expressions.ExpressionType.Paramet;
            expressionsList = new List<Transform>();
            variableClumps = new List<Transform>();
            initialized = true;
        }
    }

    public void setActiveStatus(bool status)
    {
        isActive = status;
    }

    public bool getActiveStatus()
    {
        return isActive;
    }

    public Scroll getScroll()
    {
        return scroll;
    }

    public void setButtonInputColor(Color col)
    {
        foreach (Transform t in expressionsList)
        {
            t.Find("Button_Input").GetComponent<HighlightOnRaycast>().setDefaultColor(col);
        }
    }

    public void setElementQuadtex(Texture tex)
    {
        foreach (KeyValuePair<string, Transform> t in variables)
        {
            t.Value.GetChild(0).Find("Quad").GetComponent<Renderer>().material.SetTexture("_MainTex", tex);
        }
    }

    public void setTextColor(Color c)
    {
        foreach (Transform t in expressionsList)
        {
            foreach (Transform child in t)
            {
                if (child.GetComponent<TMPro.TextMeshPro>())
                {
                    child.GetComponent<TMPro.TextMeshPro>().color = c;
                }
            }
        }

        foreach (KeyValuePair<string, Transform> t in variables)
        {
            foreach (Transform child in t.Value)
            {
                foreach (Transform gchild in child)
                {
                    if (gchild.GetComponent<TMPro.TextMeshPro>())
                    {
                        gchild.GetComponent<TMPro.TextMeshPro>().color = c;
                    }
                }
            }
        }
    }

    public void setSeparator(Transform sep)
    {
        separator = sep;
    }

    public Transform getSeparator()
    {
        return separator;
    }

    public ExpressionSet getExpSet()
    {
        return expSet;
    }

    public void addExpression(Transform expr)
    {
        expressionsList.Add(expr);
    }

    public void addVariable(string varName, Transform varValue)
    {
        if (hiddenVariables.ContainsKey(varName))
        {
            Transform temp = hiddenVariables[varName];
            temp.gameObject.SetActive(true);
            varValue = temp;
            hiddenVariables.Remove(varName);
        }

        if (variables.Count % 2 == 0)
        {
            addNewVariableClump(varValue);
        }
        else
        {
            addToVarClump(varValue);
        }

        variables.Add(varName, varValue);
    }

    public void ReAddVariable(string varName)
    {


    }

    public Expressions.ExpressionType getType()
    {
        return type;
    }

    //TODO: fix the fade in from scroll (fading in too early, when it's still out of the board's dimensions
    //      - make it so that objects don't start fading in until they're inside the board's dimensions
    public void deleteExpressionFromScroll()
    {
        variableClumps.Add(separator);
        scroll.deleteObjects(variableClumps);
        scroll.deleteObjects(expressionsList);
    }

    public void deleteVariable(List<string> vars)
    {
        varsToDelete = vars;
        deleteVar = true;
        //print("<color=red>deleting VAR</color>: " + deleteVar);
    }

    private void addToVarClump(Transform var)
    {
        var.SetParent(variableClumps[variableClumps.Count - 1]);
        var.localPosition = new Vector3(xPos, 0, 0);
    }

    private void addNewVariableClump(Transform var)
    {
        int lastComponentInd = (variableClumps.Count > 0) ?
                           scroll.getIndex(variableClumps[variableClumps.Count - 1]) : scroll.getIndex(expressionsList[2]);

        Transform newVarClump = new GameObject().transform;
        newVarClump.name = "Var Clump";
        newVarClump.localScale = Vector3.one;
        newVarClump.localPosition = Vector3.zero;
        newVarClump.localEulerAngles = Vector3.zero;
        scroll.addToScroll(null, newVarClump, lastComponentInd + 1);
        variableClumps.Add(newVarClump);

        var.SetParent(newVarClump);
        var.localPosition = new Vector3(-xPos, 0, 0);
    }

    IEnumerator MoveTo(Transform obj, Vector3 start, Vector3 end, float overTime)
    {
        float startTime = Time.time;

        while (Time.time < startTime + overTime)
        {
            if (obj == null) break;
            obj.localPosition = Vector3.Lerp(start, end, (Time.time - startTime) / overTime);
            yield return null;
        }

        if (obj != null) obj.localPosition = end;
    }

    void Update()
    {
        //TODO:
        // deactivate everything at once 
        // in destroycalled, look through each spot, if a spot is empty, find the next non empty
        // and move it to the empty spot
        // when an empty spot cant find a new slot to move, rearranging is done and delete all
        // empty clumps
        if (deleteVar)
        {
            //string varName = varsToDelete[0];

            //if (variables.ContainsKey(varName))
            //{
            //    Transform temp = variables[varName];
            //    temp.gameObject.SetActive(false);
            //    temp.SetParent(null);
            //    hiddenVariables.Add(varName, temp);
            //    variables.Remove(varName);

            //    destroyCalled = true;
            //    deleteVar = false;
            //    return;
            //}

            foreach (string s in varsToDelete)
            {
                if (variables.ContainsKey(s))
                {
                    Transform temp = variables[s];
                    temp.gameObject.SetActive(false);
                    temp.SetParent(null);
                    hiddenVariables.Add(s, temp);
                    variables.Remove(s);

                    destroyCalled = true;
                    deleteVar = false;
                    return;
                }
            }
        }

        List<int> emptyClumps = new List<int>();

        if (destroyCalled)
        {
            for (int i = 0; i < variableClumps.Count; i++)
            {
                Transform currSlot = variableClumps[i];

                if (currSlot.childCount < 2)
                {
                    if (currSlot.childCount == 1)
                    {
                        StartCoroutine(MoveTo(currSlot.GetChild(0), currSlot.GetChild(0).localPosition, new Vector3(-xPos, 0, 0), 0.3f));
                    }
                    else
                    {
                        //TODO: break out early if can't find a next slot
                        //look for next available slot
                        for (int ni = i; ni < variableClumps.Count; ni++)
                        {
                            Transform nSlot = variableClumps[ni];

                            if (nSlot.childCount > 0)
                            {
                                nSlot.GetChild(0).SetParent(currSlot);
                                StartCoroutine(MoveTo(currSlot.GetChild(0), currSlot.GetChild(0).localPosition, new Vector3(-xPos, 0, 0), 0.3f));
                                break;
                            }
                        }
                    }

                    //look for next available slot
                    for (int ni = i; ni < variableClumps.Count; ni++)
                    {
                        Transform nSlot = variableClumps[ni];

                        if (nSlot.childCount > 0)
                        {
                            nSlot.GetChild(0).SetParent(currSlot);
                            StartCoroutine(MoveTo(currSlot.GetChild(1), currSlot.GetChild(1).localPosition, new Vector3(xPos, 0, 0), 0.3f));
                            break;
                        }
                    }
                }

            }

            //BETTER IMPLEMENTATION
            int removeFrom = 0;
            for (int ind = 0; ind < variableClumps.Count; ind++)
            {
                if (variableClumps[ind].childCount == 0)
                {
                    removeFrom = ind;
                    break;
                }
            }

            if (removeFrom != 0) variableClumps.RemoveRange(removeFrom, variableClumps.Count-1);

            //POSSIBLE IMP
            //for (int ind = 0; ind < variableClumps.Count; ind++)
            //{
            //    if (variableClumps[ind].childCount == 0) emptyClumps.Add(ind);
            //}

            //foreach (int delInd in emptyClumps)
            //{
            //    Transform clump = variableClumps[delInd];
            //    variableClumps.RemoveAt(delInd);
            //    List<Transform> temp = new List<Transform>();
            //    temp.Add(clump);
            //    scroll.deleteObjects(temp);
            //}

            #region old imp
            //for (int i = 0; i < variableClumps.Count; i++)
            //{
            //    if (variableClumps[i].childCount == 1)
            //    {
            //        StartCoroutine(MoveTo(variableClumps[i].GetChild(0), variableClumps[i].GetChild(0).localPosition, new Vector3(-xPos, 0, 0), 0.3f));

            //        if (i + 1 < variableClumps.Count)
            //        {
            //            variableClumps[i + 1].GetChild(0).SetParent(variableClumps[i]);
            //            StartCoroutine(MoveTo(variableClumps[i].GetChild(1), variableClumps[i].GetChild(1).localPosition, new Vector3(xPos, 0, 0), 0.3f));
            //        }
            //    }

            //}

            //Transform last = variableClumps[variableClumps.Count - 1];

            //if (last.childCount == 0)
            //{
            //    variableClumps.Remove(last);
            //    List<Transform> temp = new List<Transform>();
            //    temp.Add(last);
            //    scroll.deleteObjects(temp);
            //}

            //destroyCalled = false;
            //print("count: " + varsToDelete.Count);
            //if (varsToDelete.Count != 0)
            //{
            //    deleteVar = true;
            //    print("<color=red>deleting VAR</color>: " + deleteVar);
            //}
            #endregion
        }
    }


}