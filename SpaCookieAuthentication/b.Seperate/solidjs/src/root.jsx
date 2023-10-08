// @refresh reload
import { Suspense } from "solid-js";
import {
  A,
  Body,
  ErrorBoundary,
  FileRoutes,
  Head,
  Html,
  Meta,
  Routes,
  Scripts,
  Title,
} from "solid-start";
import "./root.css";
export default function Root() {
  const login = () => fetch("https://localhost:7010/api/login", { method:'post', credentials:'include' });
  const test = () => fetch("https://localhost:7010/api/test", {  credentials:'include' }); 
  return (
    <Html lang="en">
      <Head>
        <Title>SolidStart - Bare</Title>
        <Meta charset="utf-8" />
        <Meta name="viewport" content="width=device-width, initial-scale=1" />
      </Head>
      <Body> 
        <button onClick={login}>Login</button>
        <button onClick={test}>Test</button>
      </Body>
    </Html>
  );
}
