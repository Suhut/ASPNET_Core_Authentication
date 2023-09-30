import { useState } from 'react'
import reactLogo from './assets/react.svg'
import viteLogo from '/vite.svg'
import './App.css'


function App() {
  const [msg, setMsg] = useState('')

  const loginJson=()=>fetch('https://api.company.local/login', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    credentials: 'include',
    body: JSON.stringify({
      Username: 'foo',
      Password: 'pw',
    })
  });

const secret = () => fetch ('https://api.company.local/protected', {
    credentials: 'include',
  })
    .then(x=>x.text())
    .then(t=> setMsg(t));

const get = () => fetch('https://api.company.local')
        .then(x=>x.text())
        .then(t=>setMsg(t));


  return (
    <> 
      <h1>Vite + React</h1>
      <div className="card">
        <div>  
          <button onClick={get}>
              Get
          </button>  
        </div>
        <div>  
          <button onClick={secret}>
             Fetch Secret
          </button>  
        </div>
        <div> 
          <button onClick={loginJson}>
              Login Json
          </button>   
        </div>
      </div>   
      <h1>{msg}</h1>
    </>
  )
}

export default App
