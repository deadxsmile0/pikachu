using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokemonObjectScript : MonoBehaviour {
    public Pokemon vPokemon;
    public SpriteRenderer vSpriteRenderer, vSpriteRendererBackGround;
    public BoxCollider2D vBoxCol2D;
    public bool vIsActive;
    private void Awake() {
        vSpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        vSpriteRendererBackGround = gameObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
        vBoxCol2D = gameObject.GetComponent<BoxCollider2D>();
        ShowPokemon();
    }

    public void ChangeThisGameObjectName(string _name) {
        gameObject.name = _name;
    }
    public void ChangeToItPokemonName() {
        if (vPokemon.PokemonName.Length == 0) {
            Debug.Log("Null name, recheck pls.");
        } else {
            gameObject.name = vPokemon.PokemonName;
            Debug.Log("Pokemon name of this GameObject is : " + vPokemon.PokemonName);
        }
    }

    public void ChangeBackgroundColor(Color _color) {
        vSpriteRendererBackGround.color = _color;
    }

    public void HidePokemon() {
        vSpriteRenderer.enabled = false;
        vSpriteRendererBackGround.enabled = false;
        vBoxCol2D.enabled = false;
        vIsActive = false;
    }
    public void ShowPokemon() {
        vSpriteRenderer.enabled = true;
        vSpriteRendererBackGround.enabled = true;
        vBoxCol2D.enabled = true;
        vIsActive = true;
    }

    public bool ActiveStatus() {
        return vIsActive;
    }

    public bool ActiveStatusReverse() {
        return vIsActive ? false : true;
    }

    public class Pokemon {
        public string PokemonName;
        public Sprite PokemonSprite;
        public Vector2 PokemonPosInGrid;
        public Vector2Int PokemonCoordInArray;
        public Pokemon(Sprite _PkmSprite, Vector2 _PkmPos) {
            PokemonName = "";
            PokemonSprite = _PkmSprite;
            PokemonPosInGrid = _PkmPos;
        }
        public Pokemon(SpriteRenderer _spRender, Sprite _PkmSprite, Vector2 _PkmPos) {
            PokemonSprite = _PkmSprite;
            PokemonPosInGrid = _PkmPos;
            ChangeSpriteOfPokemon(_spRender, _PkmSprite);
            PokemonName = _PkmSprite.name;
        }

        public void ChangeSpriteOfPokemon(SpriteRenderer _spRender, Sprite _spriteToChangeTo) {
            PokemonSprite = _spriteToChangeTo;
            _spRender.sprite = _spriteToChangeTo;
        }
    }
}
