﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

public class Node {
    private const double NODE_SEPARATOR = 1;
    private const int MAX_HEIGHT = 7;
    private const int LEFT = -1;
    private const int RIGHT = 1;
    public Vector2 position;
    public Node parent;
    public Node leftChild = null;
    public Node rightChild = null;
    public int value;

    public Node() {
        leftChild = null;
        rightChild = null;
    }

    public Node Insert(Node node, ref int value, ref Vector2 rootPosition, ref Vector2 parentPosition, int n) {
        n++;
        if (n > MAX_HEIGHT) {
            throw new HeightLimitException("Max tree height reached, cant insert node.");
        }
        if (node == null || node.value == -1) {
            Debug.Log("nuevo en n = " + n + " - vale : " + value + "\n");
            node = new Node();
            node.position.x = rootPosition.x;
            node.position.y = rootPosition.y;
            node.value = value;
            Debug.Log("inserte " + node.value + "\n");
        }
        else if (value > node.value) {
            Debug.Log("pasada r en n = " + n + " - vale : " + this.value + "\n");
            parentPosition = rootPosition;
            // divido por n para q no se superpongan los hijos de nodos hermanos.
            rootPosition.x += (float)(GameManager.TREE_X_OFFSET - n/NODE_SEPARATOR);
            rootPosition.y -= GameManager.TREE_Y_OFFSET;
            Debug.Log("inserta" + value + " en derecha de " + node.value + " parent position: " + parentPosition.x + "," + parentPosition.y+"\n");
            node.rightChild = Insert(node.rightChild, ref value, ref rootPosition, ref parentPosition,n);

            /*Debug.Log("el hijo derecho de " + root.value + " es " + rightChild + " y su valor es " + rightChild.value + "\n");*/
        } else {
            Debug.Log("pasada l en n = " + n + " - vale : " + this.value + "\n");
            parentPosition = rootPosition;
            // divido por n para q no se superpongan los hijos de nodos hermanos.
            rootPosition.x -= (float)(GameManager.TREE_X_OFFSET - n/NODE_SEPARATOR);
            rootPosition.y -= GameManager.TREE_Y_OFFSET;
            Debug.Log("inserta" + value + " en izquierda de " + node.value + " parent position: " + parentPosition.x + "," + parentPosition.y+ "\n");
            node.leftChild = Insert(node.leftChild, ref value, ref rootPosition, ref parentPosition,n);
            /*Debug.Log("el hijo izquierdo de " + root.value + " es " + leftChild + " y su valor es " + leftChild.value + "\n");*/
        }

        return node;
    }

    public void PrintTree(Node root) {
        if (root == null) {
            Debug.Log("null \n");
            return;
        }
        else {
            Debug.Log(root.value);
        }

        PrintTree(root.leftChild);
        PrintTree(root.rightChild);
    }

    public Node SearchNode(Vector2 position, int value, Node root, ref int n) {
        n++;
        if (root == null) return null;
        if (root.position == position) return root;
        else if (value > root.value) return SearchNode(position, value, root.rightChild, ref n);
        else {
            return SearchNode(position, value, root.leftChild, ref n);
        }
    }

    public void DeleteNode(ref Node node, Dictionary<string, string> textNodeUpdate, out string deletedNodeKey) {
        if (node.rightChild == null && node.leftChild == null) {
            deletedNodeKey = node.position.x.ToString(CultureInfo.InvariantCulture) + GameManager.MAGIC_KEY + node.position.y;
            //aca solo lo borro
            node = null;
        }
        else if (node.leftChild != null && node.rightChild == null) {
            //reemplazo valores del nodo por maximo hijo izquierdo
            Debug.Log("estoy borrando el-----: " + node.value + " por izquierda" + "\n");
            Debug.Log("por el numero: " + node.leftChild.value+ "\n");
            var key = node.position.x.ToString(CultureInfo.InvariantCulture) + GameManager.MAGIC_KEY + node.position.y;
            var maxLeft = FindMax(node.leftChild);
            node.value = maxLeft.value;
            textNodeUpdate.Add(key, node.value.ToString(CultureInfo.InvariantCulture));
            
            DeleteNode(ref maxLeft, textNodeUpdate, out deletedNodeKey);
        }
        else {
            //reemplazo valores del nodo por minimo hijo derecho
            Debug.Log("estoy borrando el-----: " + node.value + " por derecha" + "\n");
            Debug.Log("por el numero: " + node.rightChild.value+ "\n");
            var key = node.position.x.ToString(CultureInfo.InvariantCulture) + GameManager.MAGIC_KEY + node.position.y;
            var minRight = FindMin(node.rightChild);
            node.value = minRight.value;
            textNodeUpdate.Add(key, node.value.ToString(CultureInfo.InvariantCulture));
            
            DeleteNode(ref minRight, textNodeUpdate, out deletedNodeKey);
        }
    }

    private void UpdatePosition(Node node, int n, int direction, Dictionary<string, string> textNodeUpdate) {
        if (node == null) return;
        
        // guardo la posicion vieja del nodo
        var key = node.position.x.ToString(CultureInfo.InvariantCulture) + GameManager.MAGIC_KEY + node.position.y;
        
        if(direction == LEFT) node.position.x += (float)(GameManager.TREE_X_OFFSET + n / NODE_SEPARATOR);
        else node.position.x -= (float)(GameManager.TREE_X_OFFSET + n / NODE_SEPARATOR);
        node.position.y -= (GameManager.TREE_Y_OFFSET);

        // guardo la posicion nueva del nodo
        var newPosition = node.position.x.ToString(CultureInfo.InvariantCulture) + GameManager.MAGIC_KEY + node.position.y;
        
        if (textNodeUpdate.ContainsKey(key)) {
            textNodeUpdate.Remove(key);
        }
        textNodeUpdate.Add(key, newPosition);
        
        UpdatePosition(node.leftChild, n + 1, direction, textNodeUpdate);
        UpdatePosition(node.rightChild, n + 1, direction, textNodeUpdate);
    }

    private Node FindMin(Node node) {
        while (true) {
            if (node.leftChild == null) {
                return node;
            }
            else {
                node = node.leftChild;
            }
        }
    }

    private Node FindMax(Node node) {
        while (true) {
            if (node.rightChild == null) {
                return node;
            }
            else {
                node = node.rightChild;
            }
        }
    }
}
