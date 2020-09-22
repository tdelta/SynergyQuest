/**
 * This file is part of the "Synergy Quest" game
 * (github.com/tdelta/SynergyQuest).
 *
 * Copyright (c) 2020
 *   Marc Arnold     (m_o_arnold@gmx.de)
 *   Martin Kerscher (martin_x@live.de)
 *   Jonas Belouadi  (jonas.belouadi@posteo.net)
 *   Anton W Haubner (anton.haubner@outlook.de)
 *
 * This program is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License as published by the Free
 * Software Foundation; either version 3 of the License, or (at your option) any
 * later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
 * FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
 * more details.
 *
 * You should have received a copy of the GNU General Public License along with
 * this program; if not, see <https://www.gnu.org/licenses>.
 *
 * Additional permission under GNU GPL version 3 section 7 apply,
 * see `LICENSE.md` at the root of this source code repository.
 */

import express from 'express';
import httpProxy from 'http-proxy';
import https from 'https';
import fs from 'fs';
import http from 'http';

import open from 'open';

const app = express();

function errorHandler(
  err: Error,
  req: http.IncomingMessage,
  res: http.ServerResponse
) {
  console.error(err);
  res.end(`Error occured: ${err.message}`);
}

// ===== Diverting any websocket traffic to ws://localhost:4242 ====

const websocketProxy = httpProxy.createProxyServer({
  target: 'ws://localhost:4242',
  changeOrigin: true,
  ws: true,
});

websocketProxy.on('error', function (err, req, res) {
  res.end('Error occurred ' + err.message);
});

// ===== Diverting React Debug connections to ws://localhost:3000/sockjs-node ====

const reactDevProxy = httpProxy.createProxyServer({
  target: 'ws://localhost:3000',
  changeOrigin: true,
  ws: true,
});

reactDevProxy.on('error', function (err, req, res) {
  res.end('Error occurred ' + err.message);
});

// ===== Diverting any "normal" HTTP traffic to 'http://localhost:3000' ====

const webProxy = httpProxy.createProxyServer({
  target: 'http://localhost:3000',
  changeOrigin: true,
});

webProxy.on('error', function (err, req, res) {
  res.end('Error occurred ' + err.message);
});

app.all('/*', (req, res) => {
  webProxy.web(req, res, {}, errorHandler);
});

app.on('error', parent => {
  console.log('some error.');
});

// ===== Serving a HTTPS server on port 8000 which will divert traffic according to the above sections ====

const server = https.createServer(
  {
    key: fs.readFileSync('../../Certificates/generated/server.key', 'utf8'),
    cert: fs.readFileSync('../../Certificates/generated/server.crt', 'utf8'),
  },
  app
);

// Allow to upgrade normal HTTPS connections to websocket connections and let the proxy handle them
server.on('upgrade', (req, socket, head) => {
  // If its a sockjs connection, it must be the React dev tools trying to reach the server.
  if (req.url === '/sockjs-node') {
    reactDevProxy.ws(req, socket, head, {}, errorHandler);
  } else {
    // Otherwise, divert to game server
    websocketProxy.ws(req, socket, head, {}, errorHandler);
  }
});

server.on('error', err => {
  console.log('Caught flash policy server socket error: ');
  console.log(err.stack);
});

server.listen(8000);

// Open react app in browser
open('https://localhost:8000').then(_ => {});

console.log(
  '\n\n=================\nView the web app at "https://localhost:8000".\n=================\n\n'
);
