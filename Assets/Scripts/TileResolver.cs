using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class TileResolver : MonoBehaviour {
    [InlineEditor(Expanded = true)] public TileVariations TileVariations;

    [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
    public List<TileEntity> ConnectBlackList;

    [FoldoutGroup("Surroundings"), HideInInspector]
    public LayerMask TileLayer;

    [FoldoutGroup("Surroundings"), HideLabel]
    public SurroundingTiles Surroundings;

    [FoldoutGroup("Advanced Options")] public float connectRange = 1.5f;

    private void Start() {
        StartCoroutine(LateStart(.5f));
    }

    private IEnumerator LateStart(float delay) {
        yield return new WaitForSeconds(delay);

        UpdateTile();
        NotifySurrounding();
    }

    [Serializable]
    public struct SurroundingTiles {
        public TileEntity[] TileEntities;

        public SurroundingTiles(TileEntity north, TileEntity east, TileEntity south, TileEntity west) {
            TileEntities = new TileEntity[4];
            TileEntities[0] = north;
            TileEntities[1] = east;
            TileEntities[2] = south;
            TileEntities[3] = west;
        }
    }

    private void OnDestroy() {
        NotifySurrounding();
    }

    private void NotifySurrounding() {
        foreach (TileEntity tileEntity in Surroundings.TileEntities) {
            if (tileEntity) tileEntity.GetComponent<TileResolver>().UpdateTile();
        }
    }

    public void UpdateTile() {
        Surroundings = new SurroundingTiles(
            CheckSide(Vector3.forward, connectRange),
            CheckSide(Vector3.right, connectRange),
            CheckSide(Vector3.back, connectRange),
            CheckSide(Vector3.left, connectRange));

        ResolveTile();
    }

    private void ResolveTile() {
        transform.eulerAngles = Vector3.zero;
        TileEntity[] Surrounded = Surroundings.TileEntities;
        int surroundingCount = 0;

        foreach (TileEntity tileEntity in Surrounded) {
            if (tileEntity) surroundingCount++;
        }

        switch (surroundingCount) {
            case 0:
                SetVariationAndRotation(TileVariations.Single, 0);
                break;
            case 1:
                if (Surrounded[0]) {
                    SetVariationAndRotation(TileVariations.End, 90);
                    break;
                }

                if (Surrounded[1]) {
                    SetVariationAndRotation(TileVariations.End, 180);
                    break;
                }

                if (Surrounded[2]) {
                    SetVariationAndRotation(TileVariations.End, 270);
                    break;
                }

                if (Surrounded[3]) {
                    SetVariationAndRotation(TileVariations.End, 0);
                }

                break;
            case 2:
                // Check if two connections are adjacent -> Corner
                if (Surrounded[0] && Surrounded[1]) {
                    SetVariationAndRotation(TileVariations.Corner, 90);
                    break;
                }

                if (Surrounded[1] && Surrounded[2]) {
                    SetVariationAndRotation(TileVariations.Corner, 180);
                    break;
                }

                if (Surrounded[2] && Surrounded[3]) {
                    SetVariationAndRotation(TileVariations.Corner, 270);
                    break;
                }

                if (Surrounded[3] && Surrounded[0]) {
                    SetVariationAndRotation(TileVariations.Corner, 0);
                    break;
                }

                // Check if two connections are opposite -> Straight
                if (Surrounded[0] && Surrounded[2]) {
                    SetVariationAndRotation(TileVariations.Straight, 90);
                    break;
                }

                if (Surrounded[1] && Surrounded[3]) {
                    SetVariationAndRotation(TileVariations.Straight, 0);
                }

                break;
            case 3:
                if (Surrounded[0] && Surrounded[1] && Surrounded[2]) {
                    transform.Rotate(Vector3.up, 90);
                    SetVariationAndRotation(TileVariations.Spllt, 90);
                    break;
                }

                if (Surrounded[1] && Surrounded[2] && Surrounded[3]) {
                    SetVariationAndRotation(TileVariations.Spllt, 180);
                    break;
                }

                if (Surrounded[2] && Surrounded[3] && Surrounded[0]) {
                    SetVariationAndRotation(TileVariations.Spllt, 270);
                    break;
                }

                if (Surrounded[3] && Surrounded[0] && Surrounded[1]) {
                    SetVariationAndRotation(TileVariations.Spllt, 0);
                }

                break;
            case 4:
                SetVariationAndRotation(TileVariations.Center, 0);
                break;
        }
    }

    private void SetVariationAndRotation(Mesh tileVariation, float rotation) {
        if (tileVariation) {
            GetComponent<MeshFilter>().mesh = tileVariation;
        }
        else {
            Debug.Log("No Variation set");
        }

        transform.Rotate(Vector3.up, rotation);
    }

    private TileEntity CheckSide(Vector3 direction, float range) {
        RaycastHit hit;

        Vector3 centerOffset = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + .5f,
            gameObject.transform.position.z);
        Physics.Raycast(centerOffset, direction, out hit, range); // ,TileLayer

        if (hit.collider == null) {
            return null;
        }


        TileEntity hitTileEntity;
        try {
            hitTileEntity = hit.transform.GetComponent<TileEntity>();
            if (!hitTileEntity) return null;
        }
        catch (NullReferenceException ex) {
            return null;
        }

        foreach (TileEntity blacklistedTileEntity in ConnectBlackList) {
            if (blacklistedTileEntity.Id.Equals(hitTileEntity.Id) || !hitTileEntity.Connectable) {
                return null;
            }
        }

        // Draw connection
        Debug.DrawLine(centerOffset, hit.point, Color.red, 5f);
        return hitTileEntity;
    }
}