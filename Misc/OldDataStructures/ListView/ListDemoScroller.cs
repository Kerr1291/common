using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Components
{
#if UNITY_EDITOR
    [CustomEditor(typeof(ListDemoScroller))]
    public class ListDemoScrollerEditor : Editor
    {
        ListDemoScroller _target;
        public int scrollToIndex = 99;
        public float demoScrollRate = 25f;

        public override void OnInspectorGUI()
        {
            _target = (ListDemoScroller)target;

            scrollToIndex = EditorGUILayout.IntField("Scroll to index:", scrollToIndex);
            demoScrollRate = EditorGUILayout.FloatField("Scroll rate:", demoScrollRate);

            if( GUILayout.Button("Do Scroll To") )
            {
                Vector3 scrollDir = Vector3.down;
                int current = (_target.listView as GameDataList).GetDataIndexAtViewIndex(0);
                if(current < scrollToIndex)
                    scrollDir = Vector3.up;

                _target.stopIndex = scrollToIndex;
                _target.CurrentScrollingRate = scrollDir.y * demoScrollRate;
            }

            if( GUILayout.Button( "Advance Story" ) )
            {
                _target.AdvanceStory();
            }

            if( GUILayout.Button( "Scroll Story" ) )
            {
                _target.ScrollStory();
            }

                base.OnInspectorGUI();
        }
    }
#endif


    public class ListDemoScroller : ListViewScroller
    {
        public Text scoreCounter;

        public GameTimer ignoreTimer;
        public GameTimer extraPackageTimer;
        public GameTimer advanceTimer;

        public GameTimerDelayAction spawnFun;

        //public MonitorContentGenerator storyGenerator;

        public GameObject moreObject;

        public bool active = true;
        
        public float stopSpeed = 1f;

        protected float currentScrollingRate = 0f;

        public int stopIndex = 0;

        public int minPieces = 0;

        public float CurrentScrollingRate
        {
            get
            {
                return currentScrollingRate;
            }
            set
            {
                currentScrollingRate = value;
            }
        }

        //public MonitorContentList MList {
        //    get {
        //        return ( listView as MonitorContentList );
        //    }
        //}

        bool packageOpenedPrev = false;
        void Update()
        {
            if(!active)
                return;
            HandleInput();

            if(moreObject.activeInHierarchy)
            {
                moreFader.Lock();
                float t = moreFader.TimeRemainingNormalized;

                if( t < .5 )
                {
                    moreObject.GetComponentInChildren<UnityEngine.UI.Text>().color = Color.white * ( t / .5f ) + Color.yellow * ( 1f - ( t / .5f ) );
                }
                else
                {
                    moreObject.GetComponentInChildren<UnityEngine.UI.Text>().color = Color.yellow * ( (t-.5f) / .5f ) + Color.white * ( 1f - ( ( t - .5f ) / .5f ) );
                }
            }

            if( packageOpened != packageOpenedPrev )
            {
                AdvanceStory();
            }

            packageOpenedPrev = packageOpened;

            if( !extraPackageTimer.Locked )
            {
                extraPackageTimer.Lock();

                //Piece[] parts = GameObject.FindObjectsOfType<Piece>();
                //if( parts.Length < minPieces )
                //    TryBackupDelivery();
            }
        }

        void FixedUpdate()
        {
            if(!active)
                return;
            Scroll();
        }

        protected override void HandleInput()
        {
        }

        protected override void StartScrolling()
        {
        }

        public GameTimer moreFader;
        //bool willBeep = false;
        protected override void Scroll()
        {
            if(Mathf.Approximately(currentScrollingRate, 0f))
                return;

            //UpdateMoreObject();

            //int current = (listView as MonitorContentList).GetDataIndexAtViewIndex(0);
            //if( current != stopIndex )
            //    willBeep = true;

            //if( current == stopIndex)
            //{
            //    if( willBeep && generateBeep != null )
            //    {
            //        willBeep = false;
            //        generateBeep.Play();
            //    }
            //    currentScrollingRate = 0f;
            //    return;
            //}

            listView.DataPosition += currentScrollingRate * Time.fixedDeltaTime;
            if(Mathf.Abs(currentScrollingRate) <= stopSpeed)
                currentScrollingRate = 0f;
        }

        protected override void StopScrolling()
        {
        }

        public void ClearList()
        {
            //MList.Clear();
        }

        public void Awake()
        {
            ignoreTimer.Lock();
            AdvanceStory();
        }

        public void AdvanceStory()
        {
            if( alldone )
                return;

            stopIndex = 0;
            //storyGenerator.Clear();
            //storyGenerator.GenerateNext();
            //UpdateMoreObject();
            //StartCoroutine( DoAdvanceStory() );
        }

        bool alldone = false;
        public void ShowCredits()
        {
            alldone = true;
            if( generateBeep != null )
                generateBeep.Play();
            stopIndex = 0;
            //storyGenerator.Clear();
            //StartCoroutine( DoCredits() );
        }

        public void ScrollStory()
        {
            //stopIndex = (int)Mathf.Clamp(MList.DataIndex + 1, 0, MList.listData.Count-1);
            CurrentScrollingRate = 2f;
            Scroll();
        }

        public AudioSource generateBeep;
        
        //public IEnumerator DoAdvanceStory()
        //{
        //    int i = 0;
        //    IEnumerator next = storyGenerator.GenerateNextWithDelay();
        //    while( next.MoveNext() )
        //    {
        //        UpdateMoreObject();
        //        if( generateBeep != null && i < 3 )
        //            generateBeep.Play();
        //        yield return new WaitForSeconds( .5f );
        //        ++i;
        //    }
        //    UpdateMoreObject();
        //}

        //public IEnumerator DoCredits()
        //{
        //    int i = 0;
        //    IEnumerator next = storyGenerator.GenerateCredits();
        //    while( next.MoveNext() )
        //    {
        //        UpdateMoreObject();
        //        yield return new WaitForSeconds( .5f );
        //        ++i;
        //    }
        //    UpdateMoreObject();
        //}

        //public void UpdateMoreObject()
        //{
        //    if( MList.listData.Count <= 3 )
        //    {
        //        moreObject.SetActive( false );
        //        return;
        //    }

        //    int current = (listView as MonitorContentList).GetDataIndexAtViewIndex(2);
        //    if( current >= MList.listData.Count - 1 )
        //        moreObject.SetActive( false );
        //    else
        //        moreObject.SetActive( true );
        //}

        public static bool packageOpened = false;
        //bool order0 = true;
        bool wantRed = false;
        //bool firstScore = false;
        //int numOrders = 0;
        //bool part4 = false;
        //bool part5 = false;

        public void RedPressed()
        {
            if( wantRed )
            {
                wantRed = false;
                AdvanceStory();
            }
        }

        public GameTimer cooldownTimer;

        public void YellowPressed()
        {
            //if( ignoreTimer.Locked )
            //    return;

            //if( cooldownTimer.Locked )
            //    return;

            //FindObjectOfType<DeliveryTube>().CloseChute();

            //if( storyGenerator.StoryDone )
            //{
            //    ShowCredits();
            //}

            //cooldownTimer.Lock();

            //if( DeliveryTube.fireIsOn )
            //    return;

            //if( order0 )
            //{
            //    TryConsoleDelivery();
            //    order0 = false;
            //}

            //if(firstScore)
            //{
            //    TryConsoleDelivery();
            //}
        }

        public void GreenPressed()
        {
            ScrollStory();
        }

        //public ConsoleDelivery console;

        public void TryConsoleDelivery()
        {
            //if( order0 )
            //{
            //    numOrders++;
            //    console.OnOrderPlaced();
            //    minPieces += 5;
            //    SnapScore.Instance.score -= 10;
            //    //spawnFun.lockAction = SpawnFun;
            //    //spawnFun.Lock();
            //}
            //else
            //{
            //    numOrders++;
            //    SnapScore.Instance.score -= 10;
            //    console.OnOrderPlaced();
            //}

            //if( !part4 && numOrders >= 2 && SnapScore.Instance.score > 20 )
            //{
            //    AdvanceStory();
            //    part4 = true;
            //}
            //if( !part5 && numOrders >= 3 && SnapScore.Instance.score > 40 )
            //{
            //    part5 = true;
            //    AdvanceStory();
            //    spawnFun.lockAction = SpawnFun;
            //    spawnFun.Lock();
            //}
            //if( !advanceTimer.Locked && numOrders >= 7 )
            //{
            //    advanceTimer.Lock();
            //    AdvanceStory();
            //}
        }
        //public Transform funSpot;
        //public void SpawnFun()
        //{
        //    StartCoroutine( DoSpawnFun() );
        //    //for( int i = 0; i < 100; ++i )
        //    //{
        //    //    GameObject snapItem = Instantiate( SnapScore.Instance.ScorePrefab );
        //    //    snapItem.SetActive( true );
        //    //    snapItem.GetComponent<SnapScoreItem>().score = 1;
        //    //    snapItem.transform.position = funSpot.position;
        //    //}
        //}

        //public IEnumerator DoSpawnFun()
        //{
        //    //for( int i = 0; i < 200; ++i )
        //    //{
        //    //    GameObject snapItem = Instantiate( SnapScore.Instance.ScorePrefab );
        //    //    snapItem.SetActive( true );
        //    //    snapItem.GetComponent<SnapScoreItem>().score = 1;
        //    //    snapItem.transform.position = funSpot.position;
        //    //    yield return new WaitForEndOfFrame();
        //    //}
        //}

        public void TryBackupDelivery()
        {
            //if( DeliveryTube.fireIsOn )
            //    return;

            //FindObjectOfType<DeliveryTube>().CloseChute();
            //console.OnOrderPlaced();
        }

        //public void NotifySnap( SnapScore score )
        //{
        //    //scoreCounter.text = "1RC Membership Points: "+score.score;

        //    //if(!firstScore && score.score > 10)
        //    //{
        //    //    firstScore = true;
        //    //    AdvanceStory();
        //    //}
        //}
    }
}
