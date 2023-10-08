'use client';

import Image from 'next/image'
import { use } from 'react';

export default function Home() {
  const login = () => fetch("https://localhost:7195/api/login", { method:'post', credentials:'include' });
  const test = () => fetch("https://localhost:7195/api/test", {  credentials:'include' }); 
  return (
    <main className="flex min-h-screen flex-col items-centerp-24"> 
        <button  onClick={login}>Login</button>
        <button onClick={test}>Test</button>
    </main>
  )
}
