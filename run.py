#!/usr/bin/env python3
"""Run Guess43 locally: PostgreSQL (Docker) + ASP.NET Core API + React (Vite) dev server.

Usage:
    python run.py               # start db (Docker), backend, and frontend
    python run.py --no-db       # skip Docker; use an existing PostgreSQL
    python run.py --skip-install# skip `npm install` even if node_modules is missing
    python run.py --backend-only
    python run.py --frontend-only

Press Ctrl+C to stop everything cleanly.

URLs:
    Frontend (Vite):  http://localhost:5173   (proxies /api -> backend)
    Backend  (API):   http://localhost:5063
    Swagger:          http://localhost:5063/swagger
"""

from __future__ import annotations

import argparse
import os
import shutil
import signal
import subprocess
import sys
import threading
import time
from pathlib import Path

ROOT = Path(__file__).resolve().parent
BACKEND_PROJECT = ROOT / "src" / "backend" / "Guess43.Api"
FRONTEND_DIR = ROOT / "src" / "frontend" / "guess43-web"

IS_WINDOWS = os.name == "nt"

# ANSI colours for prefixed, interleaved output.
COLOURS = {"db": "\033[35m", "backend": "\033[36m", "frontend": "\033[32m", "sys": "\033[33m"}
RESET = "\033[0m"

_procs: list[subprocess.Popen] = []
_stopping = threading.Event()


def log(scope: str, message: str) -> None:
    colour = COLOURS.get(scope, "")
    print(f"{colour}[{scope}]{RESET} {message}", flush=True)


def which(cmd: str) -> str | None:
    """Resolve an executable, accounting for Windows .cmd shims (npm, npx)."""
    if IS_WINDOWS and cmd in {"npm", "npx", "docker"}:
        return shutil.which(cmd) or shutil.which(cmd + ".cmd")
    return shutil.which(cmd)


def require(cmd: str) -> str:
    resolved = which(cmd)
    if not resolved:
        log("sys", f"ERROR: '{cmd}' was not found on PATH. Please install it and retry.")
        sys.exit(1)
    return resolved


def stream_output(proc: subprocess.Popen, scope: str) -> None:
    """Forward a child process's stdout to the console with a scope prefix."""
    assert proc.stdout is not None
    for raw in iter(proc.stdout.readline, ""):
        if raw:
            print(f"{COLOURS.get(scope, '')}[{scope}]{RESET} {raw.rstrip()}", flush=True)
    proc.stdout.close()


def spawn(scope: str, args: list[str], cwd: Path, env: dict[str, str] | None = None) -> subprocess.Popen:
    log("sys", f"starting {scope}: {' '.join(args)}")
    proc = subprocess.Popen(
        args,
        cwd=str(cwd),
        env={**os.environ, **(env or {})},
        stdout=subprocess.PIPE,
        stderr=subprocess.STDOUT,
        text=True,
        bufsize=1,
    )
    _procs.append(proc)
    threading.Thread(target=stream_output, args=(proc, scope), daemon=True).start()
    return proc


def start_database() -> None:
    docker = require("docker")
    log("db", "starting PostgreSQL via docker compose...")
    result = subprocess.run(
        [docker, "compose", "up", "-d", "db"],
        cwd=str(ROOT),
        text=True,
        capture_output=True,
    )
    if result.returncode != 0:
        log("db", "ERROR: failed to start the database container:")
        log("db", (result.stderr or result.stdout).strip())
        log("db", "If you have your own PostgreSQL, re-run with --no-db.")
        sys.exit(1)

    # Wait until the container reports healthy (compose healthcheck: pg_isready).
    log("db", "waiting for PostgreSQL to become healthy...")
    for _ in range(60):
        check = subprocess.run(
            [docker, "inspect", "-f", "{{.State.Health.Status}}", "guess43-db-1"],
            text=True,
            capture_output=True,
        )
        status = check.stdout.strip()
        if status == "healthy":
            log("db", "PostgreSQL is healthy.")
            return
        if _stopping.is_set():
            return
        time.sleep(1)
    log("db", "WARNING: database did not report healthy in time; continuing anyway.")


def ensure_frontend_deps(skip_install: bool) -> None:
    if skip_install:
        return
    if (FRONTEND_DIR / "node_modules").exists():
        return
    npm = require("npm")
    log("frontend", "installing dependencies (npm ci)...")
    lock = FRONTEND_DIR / "package-lock.json"
    cmd = [npm, "ci"] if lock.exists() else [npm, "install"]
    result = subprocess.run(cmd, cwd=str(FRONTEND_DIR))
    if result.returncode != 0:
        log("frontend", "ERROR: dependency installation failed.")
        sys.exit(1)


def start_backend() -> subprocess.Popen:
    dotnet = require("dotnet")
    env = {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "ASPNETCORE_URLS": "http://localhost:5063",
    }
    return spawn("backend", [dotnet, "run", "--project", str(BACKEND_PROJECT)], ROOT, env)


def start_frontend() -> subprocess.Popen:
    npm = require("npm")
    return spawn("frontend", [npm, "run", "dev"], FRONTEND_DIR)


def shutdown(*_: object) -> None:
    if _stopping.is_set():
        return
    _stopping.set()
    log("sys", "shutting down... (Ctrl+C)")
    for proc in _procs:
        if proc.poll() is None:
            try:
                if IS_WINDOWS:
                    proc.terminate()
                else:
                    proc.send_signal(signal.SIGINT)
            except Exception:
                pass
    deadline = time.time() + 10
    for proc in _procs:
        remaining = max(0, deadline - time.time())
        try:
            proc.wait(timeout=remaining)
        except Exception:
            try:
                proc.kill()
            except Exception:
                pass
    log("sys", "all processes stopped.")


def main() -> int:
    parser = argparse.ArgumentParser(description="Run the Guess43 backend and frontend.")
    parser.add_argument("--no-db", action="store_true", help="Do not start PostgreSQL via Docker.")
    parser.add_argument("--skip-install", action="store_true", help="Skip npm install.")
    parser.add_argument("--backend-only", action="store_true", help="Run only the backend API.")
    parser.add_argument("--frontend-only", action="store_true", help="Run only the frontend dev server.")
    args = parser.parse_args()

    signal.signal(signal.SIGINT, shutdown)
    if hasattr(signal, "SIGTERM"):
        signal.signal(signal.SIGTERM, shutdown)

    run_backend = not args.frontend_only
    run_frontend = not args.backend_only

    if run_backend and not args.no_db:
        start_database()

    if run_frontend:
        ensure_frontend_deps(args.skip_install)

    if run_backend:
        start_backend()
        # Give the API a moment to bind before Vite starts proxying to it.
        time.sleep(2)
    if run_frontend:
        start_frontend()

    log("sys", "Guess43 is starting.")
    log("sys", "Frontend: http://localhost:5173   Backend: http://localhost:5063   Swagger: http://localhost:5063/swagger")
    log("sys", "Press Ctrl+C to stop.")

    try:
        while not _stopping.is_set():
            for proc in list(_procs):
                code = proc.poll()
                if code is not None and not _stopping.is_set():
                    log("sys", f"a process exited with code {code}; shutting everything down.")
                    shutdown()
                    return code or 0
            time.sleep(0.5)
    except KeyboardInterrupt:
        shutdown()
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
