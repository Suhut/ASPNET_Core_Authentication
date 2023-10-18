import React, { useEffect, useState } from "react";

export default function App() {
   const [user, setUser] = useState('')
   const [youtubeData, setYoutubeData] = useState('')
   const [yarpYoutubeData, setYarpYoutubeData] = useState('')

   
useEffect(() => {
    loadUserInfo();
}, []);

  const loadUserInfo = () => {
      return fetch('/api/user', { 
        headers: {
          'X-Requested-With': 'XMLHttpRequest'
        }  
      }).then(res => {
        if(res.status===401)   {
          return;
        }

        return res.json()
            .then(json=> { 
              setUser( {
                  authenticated:true,
                  id:json.id,
                  youtubeEnable : json.ytEnabled
              } )
            })
      }

      )
  }

 

  const login = () => {
    return fetch('/api/login')
      .then(r=> {
        loadUserInfo();
      });
  }; 

  const connectYoutube = () => {
    window.location = '/api/youtube-connect';
  };

  const fetchYoutubeData = () => {
    return fetch('/api-yt')
      .then(res => {
        return res.json()
          .then(json=> { 
            setYoutubeData(json);
          })
      });
  };

const fetchYarpYoutubeData = () => {
    return fetch('/yarp-api-yt/userinfo')
    .then(res => {
      return res.json()
        .then(json=> { 
          setYarpYoutubeData(json);
        })
    });
};

  return (
    <>
      <h1>Vite + React</h1>
      <div className="card">
        <div>
          <button onClick={login}>
            Login
          </button>
        </div>
        {!user.youtubeEnable? <div>
          <button onClick={connectYoutube}>
            Connect Youtube
          </button>
        </div>:<><div>
          <button onClick={fetchYoutubeData}>
            Fetch Youtube Data
          </button>
        </div>
        <div>
        <button onClick={fetchYarpYoutubeData}>
            YARP Fetch Youtube Data
          </button>
        </div></>} 
        
        <h2>USER : {JSON.stringify(user)}</h2>
        <h2>YOUTUBEDATA : {JSON.stringify(youtubeData)}</h2>
        <h2>YARP YOUTUBEDATA : {JSON.stringify(yarpYoutubeData)}</h2>
      </div>
    </>
  );
}
