import solid from "solid-start/vite";
import { defineConfig } from "vite";

import dns from 'dns';
import { readFileSync } from "fs"; 
import { resolve } from "path";

export default defineConfig({
  plugins: [
    solid({
      ssr: false,
    }),
  ],

  server: {
    port: 3000,
    https : {
      key: readFileSync(resolve('localhost-key.pem')),
      cert: readFileSync(resolve('localhost.pem'))
    }
  }
});
