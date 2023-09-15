using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class NeedySilhouettesScript : MonoBehaviour {

    public KMAudio Audio;
    public KMNeedyModule needy;
    public List<KMSelectable> rotbuttons;
    public List<KMSelectable> gridbuttons;
    public Renderer[] solids;
    public Renderer rcpatch;
    public GameObject rotcentre;
    public TextMesh[] coordinates;
    public IEnumerator[] rotate = new IEnumerator[6];

    private string[] solidnames = new string[64] {"Sphere", "Cube", "Cylinder", "Annular Cylinder", "Bicone", "Bicylinder", "Bilinski Dodecahedron", "Cone",
                                                  "Circular Frustum", "Cuboctohedron", "Deltoidal Icositetrahedron", "Bisphere", "Dodecahedron", "Egg", "Gyroelongated Square Bipyramid", "Perpendicular Cylindrical Wedge",
                                                  "Hexagonal Prism", "Hyperbolic Bicylinder", "Hyperbolic Cylinder", "Hyperboloid", "Icosahedron", "Icosidodecahedron", "Double Hyperbolic Cylinder", "Elongated Bicone",
                                                  "Elongated Pentagonal Bipyramid", "Elongated Square Bipyramid", "Napkin Ring", "Oblate Spheroid", "Octohedron", "Orthogonal Tricylinder", "Pentagonal Antiprism", "Pentagonal Bipyramid",
                                                  "Pentagonal Prism", "Pentagonal Pyramid", "Planar Tricylinder", "Prolate Spheroid", "Psuedocuboctohedron", "Psuedorhombicuboctohedron", "Reuleaux Cone", "Reuleaux Tetrahedron",
                                                  "Reuleaux Triangular Prism", "Rhombic Dodecahedron", "Rhombic Icosahedron", "Rhombic Triacontahedron", "Rhombicuboctohedron", "Rhombohedron", "Rhombo-triangular Dodecahedron", "Scutoid",
                                                  "Snub Disphenoid", "Spherical Cone", "Spherical Square Pyramid", "Sperical Segement", "Spherical Wedge", "Square Antiprism", "Square Frustum", "Tetrahedron",
                                                  "Tetrakis Triangular Prism", "Torus", "Trapezohedron", "Triakis Tetrahedron", "Triangular Bipyramid", "Triangular Prism", "Trisphere", "Parallel Cylindrical Wedge"};
    private int[] currentpos = new int[2];
    private int targetpos;
    private bool active;

    private static int moduleIDcounter = 1;
    private int moduleID;

    private void Awake()
    {
        moduleID = moduleIDcounter++;
        needy.OnNeedyActivation += Activate;
        needy.OnTimerExpired += Deactivate;
        foreach(KMSelectable rbutton in rotbuttons)
        {
            int r = rotbuttons.IndexOf(rbutton);
            rbutton.OnInteract += delegate () { rotate[r] = Rotate(r); StartCoroutine(rotate[r]); return false; };
            rbutton.OnInteractEnded += delegate () { StopCoroutine(rotate[r]); };
        }
        foreach(KMSelectable gbutton in gridbuttons)
        {
            int g = gridbuttons.IndexOf(gbutton);
            gbutton.OnInteract += delegate () { Shift(g); return false; };
        }
    }

    private IEnumerator Rotate(int dir)
    {
        if (active)
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, transform);
        while (active)
        {
            switch (dir)
            {
                case 0:
                    rotcentre.transform.RotateAround(rotcentre.transform.position, Vector3.right, 9);
                    break;
                case 1:
                    rotcentre.transform.RotateAround(rotcentre.transform.position, Vector3.up, 9);
                    break;
                case 2:
                    rotcentre.transform.RotateAround(rotcentre.transform.position, Vector3.left, 9);
                    break;
                case 3:
                    rotcentre.transform.RotateAround(rotcentre.transform.position, Vector3.down, 9);
                    break;
                case 4:
                    rotcentre.transform.RotateAround(rotcentre.transform.position, Vector3.forward, 9);
                    break;
                default:
                    rotcentre.transform.RotateAround(rotcentre.transform.position, Vector3.back, 9);
                    break;
            }
            yield return new WaitForSeconds(0.05f);
        }
    }

    private void Shift(int b)
    {
        if (active)
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
            gridbuttons[b].AddInteractionPunch(0.5f);
            currentpos[b / 2] += b % 2 == 1 ? 1 : 7;
            currentpos[b / 2] %= 8;
            coordinates[0].text = "ABCDEFGH"[currentpos[0]].ToString();
            coordinates[1].text = (currentpos[1] + 1).ToString();
        }
    }

    private void Activate()
    {
        targetpos = Random.Range(0, 64);
        currentpos[0] = Random.Range(0, 8);
        currentpos[1] = Random.Range(0, 8);
        coordinates[0].text = "ABCDEFGH"[currentpos[0]].ToString();
        coordinates[1].text = (currentpos[1] + 1).ToString();
        int[] rotation = new int[3] { Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360) };
        rotcentre.transform.RotateAround(rotcentre.transform.position, Vector3.up, rotation[0]);
        rotcentre.transform.RotateAround(rotcentre.transform.position, Vector3.right, rotation[1]);
        rotcentre.transform.RotateAround(rotcentre.transform.position, Vector3.forward, rotation[2]);
        solids[targetpos].enabled = true;
        if (targetpos == 38)
            rcpatch.enabled = true;
        active = true;
        Debug.LogFormat("[Silhouettes #{0}] The solid shown ({3}) is located at {1}{2}", moduleID, "ABCDEFGH"[targetpos/8], (targetpos % 8) + 1, solidnames[targetpos]);
    }

    private void Deactivate()
    {
        active = false;
        coordinates[0].text = "*";
        coordinates[1].text = "*";
        solids[targetpos].enabled = false;
        rcpatch.enabled = false;
        if (currentpos[0] * 8 + currentpos[1] != targetpos)
            needy.HandleStrike();
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} u/d/l/r/a/c <0-360> [Rotates the shape. Rotations can be chained] | !{0} <a-h><1-8> [Changes coordinate]";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        string[] presses = command.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
        if (presses.Length == 1 && command.Length == 2 && "ABCDEFGH".Contains(command[0].ToString().ToUpperInvariant()) && "12345678".Contains(command[1].ToString()))
        {
            yield return null;
            while(coordinates[0].text != command[0].ToString().ToUpperInvariant())
            {
                gridbuttons[1].OnInteract();
                yield return new WaitForSeconds(0.125f);
            }
            while(coordinates[1].text != command[1].ToString())
            {
                gridbuttons[3].OnInteract();
                yield return new WaitForSeconds(0.125f);
            }
            yield break;
        }
        else if(presses.Length > 0 && presses.Length % 2 == 0)
        {
            List<string> directions = new List<string> { };
            List<int> rottimes = new List<int> { };
            for(int i = 0; i < presses.Length; i++)
            {
                if (i % 2 == 0)
                    if ("UDLRCA".Contains(presses[i].ToUpperInvariant()))
                        directions.Add(presses[i]);
                    else
                    {
                        yield return "sendtochaterror Invalid direction";
                        yield break;
                    }
                else
                {
                    int angle;
                    if(int.TryParse(presses[i], out angle))
                        rottimes.Add(angle / 3);
                    else
                    {
                        yield return "sendtochaterror Invalid angle";
                        yield break;
                    }
                }
            }
            yield return null;
            for(int i = 0; i < directions.Count; i++)
            {
                int arrow = new List<string> { "U", "R", "D", "L", "A", "C" }.IndexOf(directions[i].ToUpperInvariant());
                rotbuttons[arrow].OnInteract();
                yield return new WaitForSeconds(rottimes[i]);
                rotbuttons[arrow].OnInteractEnded();
            }
        }
        else
        {
            yield return "sendtochaterror Invalid number of arguments";
        }
    }

    private void TwitchHandleForcedSolve()
    {
        StartCoroutine(HandleAutosolve());
    }

    private IEnumerator HandleAutosolve()
    {
        while (true)
        {
            while (!active) yield return null;
            int[] targets = { targetpos / 8, targetpos % 8 };
            for (int i = 0; i < 2; i++)
            {
                int diff = targets[i] - currentpos[i];
                if (Math.Abs(diff) > 4)
                {
                    diff = Math.Abs(diff) - 8;
                    if (targets[i] < currentpos[i])
                        diff = -diff;
                }
                if (diff > 0)
                {
                    while (currentpos[i] != targets[i])
                    {
                        gridbuttons[i == 0 ? 1 : 3].OnInteract();
                        yield return new WaitForSeconds(0.125f);
                    }
                }
                else
                {
                    while (currentpos[i] != targets[i])
                    {
                        gridbuttons[i == 0 ? 0 : 2].OnInteract();
                        yield return new WaitForSeconds(0.125f);
                    }
                }
            }
            while (active) yield return null;
        }
    }
}
