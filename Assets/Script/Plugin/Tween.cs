using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
Az objektum animálja a megadott tol-ig értéket egy beállított függvény szerint.

Amire figyelni kell!
1. Az animáció startPos és endPos változójának ugyan olyan típusúnak kell lennie.
2. Lehetséges típusok: 
    int 
    float
    Vector2
    Vector3
    Color

3. Események
    onStart     - Elkezdődött az animáció
    onUpdate    - Az animáció pillanatnyi értéke
    onComplete  - Befejeződött az animáció

Az onUpdate-t muszáj megadni, mivel akkor nem sok értelme lenne az animációnak.
Az onUpdate egy object típust fog kapni amit kasztolni kell a megfelelő típusra.


Példák.

    ------------------------------------------------------------------------------
    Tween.TweenAnimation animation = new Tween.TweenAnimation();

    animation.startPos = Vector3.one * 0.001f;
    animation.endPos = Vector3.one;
    animation.easeType = Tween.EaseType.easeInElastic;
    animation.time = 1;
    animation.onStart: () => { Debug.Log("Az animáció elkezdődött."); };
    animation.onUpdate = (object o) => { Debug.Log("Az animált változó pillanatnyi értéke: " + (Vector3)o); };
    animation.onComplete: () => { Debug.Log("Az animáció véget ért."); };

    Tween.StartAnimation(animation);

    ------------------------------------------------------------------------------
    Tween.TweenAnimation animation = new Tween.TweenAnimation
        (
            startPos: Vector3.one * 0.001f,
            endPos: Vector3.one,
            easeType: Tween.EaseType.easeOutCubic,
            time: 1,
            onStart: () => { Debug.Log("Az animáció elkezdődött."); },
            onUpdate: (object o) => { Debug.Log("Az animált változó pillanatnyi értéke: " + (Vector3)o); },
            onComplete: () => { Debug.Log("Az animáció véget ért."); }
        );

    Tween.StartAnimation(animation);

Figyelem!

    Ha a start pozíció mondjuk 0 és az end pozíció 1.5, akkor az animált változó típusa float, így a start pozíciót így
    kell megadni 0f .

*/
public class Tween : MonoBehaviour {

    public static Tween instance;

    static List<TweenAnimation> listOfTweenAnimation = new List<TweenAnimation>();

    int lastFrameCount; // Melyik frame-ben futott utoljára az animáció

    void Awake() {
        instance = this;
    }

	// Update is called once per frame
	void Update ()
    {
        // Ha már volt Update ebben a frame-ben, akkor nem futtat újra
        // Ez akkor lehestésges, ha aktív Tween komponensből több is van a scene-ben.
        if (lastFrameCount == Time.frameCount)
            return;

        lastFrameCount = Time.frameCount;

        // Az összes animáción lépünk egyet
        //for (int i = 0; i < listOfTweenAnimation.Count; i++)

        for (int i = listOfTweenAnimation.Count - 1; i >= 0; i--)
        {
            TweenAnimation animation = listOfTweenAnimation[i];
            animation.actTime = Mathf.Clamp(animation.actTime + Time.deltaTime, 0, animation.time);

            float easyTypeValue = animation.time == 0 ? 1 : animation.easyTypeFunction(0, 1, animation.actTime / animation.time);

            if (animation.startPos is float)
                //animation.onUpdate(Mathf.Lerp((float)animation.startPos, (float)animation.endPos, easyTypeValue));
                animation.onUpdate(LerpFloat((float)animation.startPos, (float)animation.endPos, easyTypeValue));
            else if (animation.startPos is int)
                //animation.onUpdate((int)Mathf.Lerp((int)animation.startPos, (int)animation.endPos, easyTypeValue));
                animation.onUpdate((int)LerpFloat((int)animation.startPos, (int)animation.endPos, easyTypeValue));
            else if (animation.startPos is Vector2)
                animation.onUpdate(LerpVector2((Vector2)animation.startPos, (Vector2)animation.endPos, easyTypeValue));
            else if (animation.startPos is Vector3)
                animation.onUpdate(LerpVector3((Vector3)animation.startPos, (Vector3)animation.endPos, easyTypeValue));
            else if (animation.startPos is Color)
                animation.onUpdate(Color.Lerp((Color)animation.startPos, (Color)animation.endPos, easyTypeValue));
                    
            //animation.onUpdate(animation.easyTypeFunction(animation.startPos, animation.endPos, animation.actTime / animation.time));
            if (animation.actTime == animation.time)
            {
                animation.status = TweenAnimation.AnimationState.finished;
                listOfTweenAnimation.Remove(animation);

                if (animation.onComplete != null)
                    animation.onComplete();
            }
        }
	}

    #region newLerpFunctions
    // Ezekre azért van szükség, mert a hagyományos Lerp-eknél az érték csak 0 és 1 között lehet
    // viszont vannak olyan easyTween függvények amik nullánál kisebb illetve egynél nagyobb értékeket is visszaadnak pl. easeOutBack
    // A color animálásánál ez még nincs megoldva, mivel nincs is nagyon értelme
    float LerpFloat(float startPos, float endPos, float value)
    {
        return startPos + (endPos - startPos) * value;
    }

    Vector2 LerpVector2(Vector2 startPos, Vector2 endPos, float value)
    {
        return new Vector2(LerpFloat(startPos.x, endPos.x, value), LerpFloat(startPos.y, endPos.y, value));
    }

    Vector3 LerpVector3(Vector3 startPos, Vector3 endPos, float value)
    {
        return new Vector3(LerpFloat(startPos.x, endPos.x, value), LerpFloat(startPos.y, endPos.y, value), LerpFloat(startPos.z, endPos.z, value));
    }
    #endregion

    public static void StartAnimation(TweenAnimation animation)
    {
        if (!instance)
        {
            Debug.LogError("Az animáció valószínű, hogy nem fog futni, mivel egyik gameObject sem tartalmaz Tween komponenst");
        }

        if (animation.endPos.GetType() != animation.startPos.GetType())
        {
            Debug.LogError("Az animáció startPos és endPos értékeinek típusa nem egyezik meg! (" + animation.startPos.GetType() + ") != (" + animation.endPos.GetType() + ")");
            return;
        }

        // Beállítjuk, hogy melyik függvényt használja az animációhoz
        switch (animation.easeType)
        {
            case EaseType.nothing: break;
            case EaseType.easeInQuad: animation.easyTypeFunction = easeInQuad; break;
            case EaseType.easeOutQuad: animation.easyTypeFunction = easeOutQuad; break;
            case EaseType.easeInOutQuad: animation.easyTypeFunction = easeInOutQuad; break;
            case EaseType.easeInCubic: animation.easyTypeFunction = easeInCubic; break;
            case EaseType.easeOutCubic: animation.easyTypeFunction = easeOutCubic; break;
            case EaseType.easeInOutCubic: animation.easyTypeFunction = easeInOutCubic; break;
            case EaseType.easeInQuart: animation.easyTypeFunction = easeInQuart; break;
            case EaseType.easeOutQuart: animation.easyTypeFunction = easeOutQuart; break;
            case EaseType.easeInOutQuart: animation.easyTypeFunction = easeInOutQuart; break;
            case EaseType.easeInQuint: animation.easyTypeFunction = easeInQuint; break;
            case EaseType.easeOutQuint: animation.easyTypeFunction = easeOutQuint; break;
            case EaseType.easeInOutQuint: animation.easyTypeFunction = easeInOutQuint; break;
            case EaseType.easeInSine: animation.easyTypeFunction = easeInSine; break;
            case EaseType.easeOutSine: animation.easyTypeFunction = easeOutSine; break;
            case EaseType.easeInOutSine: animation.easyTypeFunction = easeInOutSine; break;
            case EaseType.easeInExpo: animation.easyTypeFunction = easeInExpo; break;
            case EaseType.easeOutExpo: animation.easyTypeFunction = easeOutExpo; break;
            case EaseType.easeInOutExpo: animation.easyTypeFunction = easeInOutExpo; break;
            case EaseType.easeInCirc: animation.easyTypeFunction = easeInCirc; break;
            case EaseType.easeOutCirc: animation.easyTypeFunction = easeOutCirc; break;
            case EaseType.easeInOutCirc: animation.easyTypeFunction = easeInOutCirc; break;
            case EaseType.linear: animation.easyTypeFunction = linear; break;
            case EaseType.spring: animation.easyTypeFunction = spring; break;
            case EaseType.easeInBounce: animation.easyTypeFunction = easeInBounce; break;
            case EaseType.easeOutBounce: animation.easyTypeFunction = easeOutBounce; break;
            case EaseType.easeInOutBounce: animation.easyTypeFunction = easeInOutBounce; break;
            case EaseType.easeInBack: animation.easyTypeFunction = easeInBack; break;
            case EaseType.easeOutBack: animation.easyTypeFunction = easeOutBack; break;
            case EaseType.easeInOutBack: animation.easyTypeFunction = easeInOutBack; break;
            case EaseType.easeInElastic: animation.easyTypeFunction = easeInElastic; break;
            case EaseType.easeOutElastic: animation.easyTypeFunction = easeOutElastic; break;
            case EaseType.easeInOutElastic: animation.easyTypeFunction = easeInOutElastic; break;
        }

        animation.actTime = 0;
        animation.status = TweenAnimation.AnimationState.running;

        if (!listOfTweenAnimation.Contains(animation))
            listOfTweenAnimation.Add(animation);
    }

    public static void StopAnimation(TweenAnimation animation)
    {
         listOfTweenAnimation.Remove(animation);
    }

    /// <summary>
    /// The type of easing to use based on Robert Penner's open source easing equations (http://www.robertpenner.com/easing_terms_of_use.html).
    /// </summary>
    public enum EaseType
    {
        nothing,
        easeInQuad,
        easeOutQuad,
        easeInOutQuad,
        easeInCubic,
        easeOutCubic,
        easeInOutCubic,
        easeInQuart,
        easeOutQuart,
        easeInOutQuart,
        easeInQuint,
        easeOutQuint,
        easeInOutQuint,
        easeInSine,
        easeOutSine,
        easeInOutSine,
        easeInExpo,
        easeOutExpo,
        easeInOutExpo,
        easeInCirc,
        easeOutCirc,
        easeInOutCirc,
        linear,
        spring,
        /* GFX47 MOD START */
        //bounce,
        easeInBounce,
        easeOutBounce,
        easeInOutBounce,
        /* GFX47 MOD END */
        easeInBack,
        easeOutBack,
        easeInOutBack,
        /* GFX47 MOD START */
        //elastic,
        easeInElastic,
        easeOutElastic,
        easeInOutElastic,
        /* GFX47 MOD END */
        punch
    }

    public delegate void CallBack();
    public delegate float CallBack_In_Float_Float_Float_Out_Float (float a, float b, float c);
    public delegate void CallBack_In_object(object o);

    public class TweenAnimation
    {
        public enum AnimationState
        {
            running,        // Az nimáció futás alatt
            finished        // Az animáció befejeződött
        }

        /// <summary>
        /// Mennyi idő alatt kell végbe mennie az animációnak
        /// </summary>
        public float time = 1; 

        /// <summary>
        /// Az animáció kezdő állapota
        /// </summary>
        public object startPos = 0; // A kiínduló pozíció
        /// <summary>
        /// Az animáció vég állapota
        /// </summary>
        public object endPos = 1;   // A cél pozíció

        /// <summary>
        /// Az animációhoz használt átmenet függvény
        /// </summary>
        public EaseType easeType;

        public CallBack_In_Float_Float_Float_Out_Float easyTypeFunction;
           
        /// <summary>
        /// Visszajelzés az animáció elkezdéséről
        /// </summary>
        public CallBack onStart;
        /// <summary>
        /// Az animáció pillantnyi értéke
        /// </summary>
        public CallBack_In_object onUpdate;
        /// <summary>
        /// Visszajelzés az animáció befejezéséről
        /// </summary>
        public CallBack onComplete;

        /// <summary>
        /// Hány másodperc telt el az animációban
        /// </summary>
        public float actTime;  // Hol tart az idő

        /// <summary>
        /// Milyen állapotban van az animáció
        /// </summary>
        public AnimationState status;

        //public float isAnimationRunning;   // Megy-e az animáció

        public TweenAnimation(float time = 1, object startPos = null, object endPos = null, EaseType easeType = EaseType.linear, CallBack onStart = null, CallBack_In_object onUpdate = null, CallBack onComplete = null)
        {
            this.time = time;
            this.startPos = startPos;
            this.endPos = endPos;

            this.easeType = easeType;

            this.onStart = onStart;
            this.onUpdate = onUpdate;
            this.onComplete = onComplete;
        }
    }

    #region Easing Curves

    private static float linear(float start, float end, float value)
    {
        return Mathf.Lerp(start, end, value);
    }

    private static float clerp(float start, float end, float value)
    {
        float min = 0.0f;
        float max = 360.0f;
        float half = Mathf.Abs((max - min) * 0.5f);
        float retval = 0.0f;
        float diff = 0.0f;
        if ((end - start) < -half)
        {
            diff = ((max - start) + end) * value;
            retval = start + diff;
        }
        else if ((end - start) > half)
        {
            diff = -((max - end) + start) * value;
            retval = start + diff;
        }
        else retval = start + (end - start) * value;
        return retval;
    }

    private static float spring(float start, float end, float value)
    {
        value = Mathf.Clamp01(value);
        value = (Mathf.Sin(value * Mathf.PI * (0.2f + 2.5f * value * value * value)) * Mathf.Pow(1f - value, 2.2f) + value) * (1f + (1.2f * (1f - value)));
        return start + (end - start) * value;
    }

    static float easeInQuad(float start, float end, float value)
    {
        end -= start;
        return end * value * value + start;
    }

    private static float easeOutQuad(float start, float end, float value)
    {
        end -= start;
        return -end * value * (value - 2) + start;
    }

    private static float easeInOutQuad(float start, float end, float value)
    {
        value /= .5f;
        end -= start;
        if (value < 1) return end * 0.5f * value * value + start;
        value--;
        return -end * 0.5f * (value * (value - 2) - 1) + start;
    }

    private static float easeInCubic(float start, float end, float value)
    {
        end -= start;
        return end * value * value * value + start;
    }

    private static float easeOutCubic(float start, float end, float value)
    {
        value--;
        end -= start;
        return end * (value * value * value + 1) + start;
    }

    private static float easeInOutCubic(float start, float end, float value)
    {
        value /= .5f;
        end -= start;
        if (value < 1) return end * 0.5f * value * value * value + start;
        value -= 2;
        return end * 0.5f * (value * value * value + 2) + start;
    }

    private static float easeInQuart(float start, float end, float value)
    {
        end -= start;
        return end * value * value * value * value + start;
    }

    private static float easeOutQuart(float start, float end, float value)
    {
        value--;
        end -= start;
        return -end * (value * value * value * value - 1) + start;
    }

    private static float easeInOutQuart(float start, float end, float value)
    {
        value /= .5f;
        end -= start;
        if (value < 1) return end * 0.5f * value * value * value * value + start;
        value -= 2;
        return -end * 0.5f * (value * value * value * value - 2) + start;
    }

    private static float easeInQuint(float start, float end, float value)
    {
        end -= start;
        return end * value * value * value * value * value + start;
    }

    private static float easeOutQuint(float start, float end, float value)
    {
        value--;
        end -= start;
        return end * (value * value * value * value * value + 1) + start;
    }

    private static float easeInOutQuint(float start, float end, float value)
    {
        value /= .5f;
        end -= start;
        if (value < 1) return end * 0.5f * value * value * value * value * value + start;
        value -= 2;
        return end * 0.5f * (value * value * value * value * value + 2) + start;
    }

    private static float easeInSine(float start, float end, float value)
    {
        end -= start;
        return -end * Mathf.Cos(value * (Mathf.PI * 0.5f)) + end + start;
    }

    private static float easeOutSine(float start, float end, float value)
    {
        end -= start;
        return end * Mathf.Sin(value * (Mathf.PI * 0.5f)) + start;
    }

    private static float easeInOutSine(float start, float end, float value)
    {
        end -= start;
        return -end * 0.5f * (Mathf.Cos(Mathf.PI * value) - 1) + start;
    }

    private static float easeInExpo(float start, float end, float value)
    {
        end -= start;
        return end * Mathf.Pow(2, 10 * (value - 1)) + start;
    }

    private static float easeOutExpo(float start, float end, float value)
    {
        end -= start;
        return end * (-Mathf.Pow(2, -10 * value) + 1) + start;
    }

    private static float easeInOutExpo(float start, float end, float value)
    {
        value /= .5f;
        end -= start;
        if (value < 1) return end * 0.5f * Mathf.Pow(2, 10 * (value - 1)) + start;
        value--;
        return end * 0.5f * (-Mathf.Pow(2, -10 * value) + 2) + start;
    }

    private static float easeInCirc(float start, float end, float value)
    {
        end -= start;
        return -end * (Mathf.Sqrt(1 - value * value) - 1) + start;
    }

    private static float easeOutCirc(float start, float end, float value)
    {
        value--;
        end -= start;
        return end * Mathf.Sqrt(1 - value * value) + start;
    }

    private static float easeInOutCirc(float start, float end, float value)
    {
        value /= .5f;
        end -= start;
        if (value < 1) return -end * 0.5f * (Mathf.Sqrt(1 - value * value) - 1) + start;
        value -= 2;
        return end * 0.5f * (Mathf.Sqrt(1 - value * value) + 1) + start;
    }

    /* GFX47 MOD START */
    private static float easeInBounce(float start, float end, float value)
    {
        end -= start;
        float d = 1f;
        return end - easeOutBounce(0, end, d - value) + start;
    }
    /* GFX47 MOD END */

    /* GFX47 MOD START */
    //private float bounce(float start, float end, float value){
    private static float easeOutBounce(float start, float end, float value)
    {
        value /= 1f;
        end -= start;
        if (value < (1 / 2.75f))
        {
            return end * (7.5625f * value * value) + start;
        }
        else if (value < (2 / 2.75f))
        {
            value -= (1.5f / 2.75f);
            return end * (7.5625f * (value) * value + .75f) + start;
        }
        else if (value < (2.5 / 2.75))
        {
            value -= (2.25f / 2.75f);
            return end * (7.5625f * (value) * value + .9375f) + start;
        }
        else {
            value -= (2.625f / 2.75f);
            return end * (7.5625f * (value) * value + .984375f) + start;
        }
    }
    /* GFX47 MOD END */

    /* GFX47 MOD START */
    private static float easeInOutBounce(float start, float end, float value)
    {
        end -= start;
        float d = 1f;
        if (value < d * 0.5f) return easeInBounce(0, end, value * 2) * 0.5f + start;
        else return easeOutBounce(0, end, value * 2 - d) * 0.5f + end * 0.5f + start;
    }
    /* GFX47 MOD END */

    private static float easeInBack(float start, float end, float value)
    {
        end -= start;
        value /= 1;
        float s = 1.70158f;
        return end * (value) * value * ((s + 1) * value - s) + start;
    }

    private static float easeOutBack(float start, float end, float value)
    {
        float s = 1.70158f;
        end -= start;
        value = (value) - 1;
        return end * ((value) * value * ((s + 1) * value + s) + 1) + start;
    }

    private static float easeInOutBack(float start, float end, float value)
    {
        float s = 1.70158f;
        end -= start;
        value /= .5f;
        if ((value) < 1)
        {
            s *= (1.525f);
            return end * 0.5f * (value * value * (((s) + 1) * value - s)) + start;
        }
        value -= 2;
        s *= (1.525f);
        return end * 0.5f * ((value) * value * (((s) + 1) * value + s) + 2) + start;
    }

    private static float punch(float amplitude, float value)
    {
        float s = 9;
        if (value == 0)
        {
            return 0;
        }
        else if (value == 1)
        {
            return 0;
        }
        float period = 1 * 0.3f;
        s = period / (2 * Mathf.PI) * Mathf.Asin(0);
        return (amplitude * Mathf.Pow(2, -10 * value) * Mathf.Sin((value * 1 - s) * (2 * Mathf.PI) / period));
    }

    /* GFX47 MOD START */
    private static float easeInElastic(float start, float end, float value)
    {
        end -= start;

        float d = 1f;
        float p = d * .3f;
        float s = 0;
        float a = 0;

        if (value == 0) return start;

        if ((value /= d) == 1) return start + end;

        if (a == 0f || a < Mathf.Abs(end))
        {
            a = end;
            s = p / 4;
        }
        else {
            s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
        }

        return -(a * Mathf.Pow(2, 10 * (value -= 1)) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p)) + start;
    }
    /* GFX47 MOD END */

    /* GFX47 MOD START */
    //private float elastic(float start, float end, float value){
    private static float easeOutElastic(float start, float end, float value)
    {
        /* GFX47 MOD END */
        //Thank you to rafael.marteleto for fixing this as a port over from Pedro's UnityTween
        end -= start;

        float d = 1f;
        float p = d * .3f;
        float s = 0;
        float a = 0;

        if (value == 0) return start;

        if ((value /= d) == 1) return start + end;

        if (a == 0f || a < Mathf.Abs(end))
        {
            a = end;
            s = p * 0.25f;
        }
        else {
            s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
        }

        return (a * Mathf.Pow(2, -10 * value) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p) + end + start);
    }

    /* GFX47 MOD START */
    private static float easeInOutElastic(float start, float end, float value)
    {
        end -= start;

        float d = 1f;
        float p = d * .3f;
        float s = 0;
        float a = 0;

        if (value == 0) return start;

        if ((value /= d * 0.5f) == 2) return start + end;

        if (a == 0f || a < Mathf.Abs(end))
        {
            a = end;
            s = p / 4;
        }
        else {
            s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
        }

        if (value < 1) return -0.5f * (a * Mathf.Pow(2, 10 * (value -= 1)) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p)) + start;
        return a * Mathf.Pow(2, -10 * (value -= 1)) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p) * 0.5f + end + start;
    }
    /* GFX47 MOD END */

    #endregion

}
