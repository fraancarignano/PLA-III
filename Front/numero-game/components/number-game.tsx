"use client";

import React, { useState, useRef } from "react";
import { Sparkles, UserPlus, Play } from "lucide-react";

const cn = (...args: (string | undefined | false | null)[]) => args.filter(Boolean).join(" "); 

type Attempt = {
    numbers: string;
    fama: number;
    pica: number;
    isCorrect: boolean;
};

type CardProps = {
    children: React.ReactNode;
    className?: string;
};
const Card: React.FC<CardProps> = ({ children, className }) => <div className={cn("p-6 bg-white rounded-xl shadow-lg", className)}>{children}</div>;

type CustomButtonProps = {
    children: React.ReactNode;
    onClick?: () => void;
    disabled?: boolean;
    className?: string;
    size?: "sm" | "md" | "lg";
    variant?: "default" | "outline";
    type?: "button" | "submit" | "reset";
};
const CustomButton: React.FC<CustomButtonProps> = ({ children, onClick, disabled, className, size = "md", variant = "default", type = "button" }) => {
    let baseClasses = "rounded-lg font-semibold transition-all duration-150 active:scale-[0.98]";
    let sizeClasses = size === "lg" ? "h-12 px-6 text-lg" : "h-10 px-4";
    let variantClasses = "bg-indigo-600 text-white hover:bg-indigo-700 disabled:bg-indigo-400 disabled:cursor-not-allowed";
    if (variant === "outline") variantClasses = "bg-transparent border border-gray-300 text-gray-700 hover:bg-gray-100 disabled:bg-gray-50";

    return (
        <button 
            type={type}
            onClick={onClick} 
            disabled={disabled} 
            className={cn(baseClasses, sizeClasses, variantClasses, className)}
        >
            {children}
        </button>
    );
};

const API_BASE_URL = 'http://localhost:5299/api/game/v1';

const apiFetch = async (endpoint: string, method: string, body?: any) => {
    const url = `${API_BASE_URL}${endpoint}`;
    
    const config: RequestInit = {
        method: method,
        headers: {
            'Content-Type': 'application/json',
        },
    };

    if (body) {
        config.body = JSON.stringify(body);
    }

    const maxRetries = 3;
    for (let i = 0; i < maxRetries; i++) {
        try {
            const response = await fetch(url, config);

            if (response.ok) {
                return response.json();
            } else {
                let errorData: any = null;
                try {
                    errorData = await response.json(); 
                } catch (e) {
                    // Si no es JSON, usa el texto de estado
                }
                
                const errorMessage = errorData?.message || errorData?.Message || errorData?.title || response.statusText || `Error de servidor: ${response.status}`;
                throw new Error(errorMessage);
            }
        } catch (error) {
            if (i === maxRetries - 1 || !(error instanceof TypeError)) { 
                throw error;
            }
            await new Promise(resolve => setTimeout(resolve, Math.pow(2, i) * 1000));
        }
    }
};

export default function NumberGame() {
    const [playerId, setPlayerId] = useState<number | null>(null); 
    const [gameId, setGameId] = useState<number | null>(null);     
    const [loading, setLoading] = useState<boolean>(false);
    const [message, setMessage] = useState<string>("");
    const [isError, setIsError] = useState<boolean>(false);

    const [firstName, setFirstName] = useState<string>("");
    const [lastName, setLastName] = useState<string>("");
    const [age, setAge] = useState<number | string>("");

    const [currentGuess, setCurrentGuess] = useState<string[]>(["", "", "", ""]);
    const [attempts, setAttempts] = useState<Attempt[]>([]);
    const [gameWon, setGameWon] = useState<boolean>(false);
    const MAX_ATTEMPTS = 10;

    const inputRefs = [
        useRef<HTMLInputElement>(null),
        useRef<HTMLInputElement>(null),
        useRef<HTMLInputElement>(null),
        useRef<HTMLInputElement>(null),
    ];

    const setStatusMessage = (msg: string, error: boolean = false) => {
        setMessage(msg);
        setIsError(error);
    };

    const handleLoginOrRegister = async () => {
        setLoading(true);
        setStatusMessage('Intentando registrar jugador...', false);

        if (age === "") { 
            setStatusMessage("La edad es obligatoria.", true);
            setLoading(false);
            return;
        }

        try {
            const data = await apiFetch("/register", "POST", {
                FirstName: firstName,
                LastName: lastName,
                Age: Number(age),
            });
            
            const newPlayerId = data.playerId || data.PlayerId;
            setPlayerId(newPlayerId);
            setFirstName("");
            setLastName("");
            setAge("");

            setStatusMessage(`¡Registro exitoso! Player ID: ${newPlayerId}. Ahora puedes iniciar un juego.`, false);
        
        } catch (error) {
            let msg = 'Error de conexión/registro. Asegúrate que el backend esté corriendo.';
            if (error instanceof Error) {
                 msg = `Fallo al registrar: ${error.message}`;
            }
            setStatusMessage(msg, true);
        } finally {
            setLoading(false);
        }
    };

    const startGame = async () => {
        if (playerId === null) return;
        
        setLoading(true);
        setStatusMessage('Iniciando nuevo juego...', false);
        setAttempts([]);
        setGameWon(false);

        try {
            const data = await apiFetch("/start", "POST", {
                PlayerId: playerId,
            });

            const newGameId = data.gameId || data.GameId;
            setGameId(newGameId);
            setStatusMessage(`Juego ID ${newGameId} iniciado. ¡A adivinar!`, false);
            
        } catch (error) {
            let msg = 'Error al iniciar el juego.';
            if (error instanceof Error) {
                 msg = `Fallo al iniciar el juego: ${error.message}`;
            }
            setStatusMessage(msg, true);
        } finally {
            setLoading(false);
        }
    };
    
    const handleSubmit = async () => {
        if (gameId === null || currentGuess.some((digit) => digit === "")) return;
        
        const attemptedNumber = currentGuess.join("");

        if (new Set(attemptedNumber.split('')).size !== 4) {
            setStatusMessage("Error: Los números no pueden repetirse.", true);
            return;
        }
        
        setLoading(true);
        setStatusMessage('Verificando intento...', false);

        try {
            const data = await apiFetch("/guess", "POST", {
                GameId: gameId,
                AttemptedNumber: attemptedNumber,
            });

            const picas = data.picas || data.Picas || 0;
            const famas = data.famas || data.Famas || 0;
            const responseMessage = data.message || data.Message || "";
            
            const isCorrect = responseMessage.toLowerCase().includes("felicidades") || famas === 4;

            const newAttempt: Attempt = {
                numbers: attemptedNumber,
                fama: famas,
                pica: picas,
                isCorrect,
            };

            setAttempts((prev) => [...prev, newAttempt]);
            setCurrentGuess(["", "", "", ""]);
            inputRefs[0].current?.focus();

            if (isCorrect) {
                setGameWon(true);
                setGameId(null);
                setStatusMessage('🎉 ¡FELICITACIONES! Has ganado.', false);
            } else if (attempts.length + 1 >= MAX_ATTEMPTS) {
                setGameId(null);
                setStatusMessage(`😭 Juego Terminado. Has agotado los ${MAX_ATTEMPTS} intentos.`, true);
            } else {
                setStatusMessage(responseMessage, false);
            }

        } catch (error) {
            let msg = 'Error al procesar el intento.';
            if (error instanceof Error) {
                 msg = `Fallo al adivinar: ${error.message}`;
            }
            setStatusMessage(msg, true);
        } finally {
            setLoading(false);
        }
    };

    const handleInputChange = (index: number, value: string) => {
        if (value.length > 1) return;
        if (value !== "" && (isNaN(Number(value)) || Number(value) < 0 || Number(value) > 9)) return;

        const newGuess = [...currentGuess];
        newGuess[index] = value;
        setCurrentGuess(newGuess);

        if (value !== "" && index < 3) {
            inputRefs[index + 1].current?.focus();
        }
    };

    const handleKeyDown = (index: number, e: React.KeyboardEvent) => {
        if (e.key === "Backspace" && currentGuess[index] === "" && index > 0) {
            inputRefs[index - 1].current?.focus();
        }
        if (e.key === "Enter" && !loading && !gameWon && gameId) {
            handleSubmit();
        }
    };
    
    if (playerId === null) {
        return (
            <div className="min-h-screen flex items-center justify-center p-4 bg-gray-50">
                <Card className="w-full max-w-sm p-8 space-y-6">
                    <div className="text-center">
                        <h2 className="text-2xl font-bold text-gray-800">Registro de Jugador</h2>
                        <p className="text-sm text-gray-500 mt-1">Completa tus datos para crear tu perfil de jugador.</p>
                    </div>
                    <div className="space-y-4">
                        <div>
                            <label className="block text-sm font-medium text-gray-700">Nombre</label>
                            <input
                                type="text"
                                value={firstName}
                                onChange={(e) => setFirstName(e.target.value)}
                                disabled={loading}
                                className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-lg shadow-sm focus:ring-indigo-500 focus:border-indigo-500"
                                required
                            />
                        </div>
                        <div>
                            <label className="block text-sm font-medium text-gray-700">Apellido</label>
                            <input
                                type="text"
                                value={lastName}
                                onChange={(e) => setLastName(e.target.value)}
                                disabled={loading}
                                className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-lg shadow-sm focus:ring-indigo-500 focus:border-indigo-500"
                                required
                            />
                        </div>
                        <div>
                            <label className="block text-sm font-medium text-gray-700">Edad</label>
                            <input
                                type="number"
                                value={age}
                                onChange={(e) => setAge(e.target.value)}
                                min="1"
                                max="150"
                                disabled={loading}
                                className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-lg shadow-sm focus:ring-indigo-500 focus:border-indigo-500"
                                required
                            />
                        </div>
                        <CustomButton onClick={handleLoginOrRegister} disabled={loading || !firstName || !lastName || !age} className="w-full">
                            {loading ? 'Registrando...' : <span className="flex items-center justify-center"><UserPlus className="w-4 h-4 mr-2" /> Registrar Perfil</span>}
                        </CustomButton>
                    </div>
                    {message && (
                        <div className={`mt-4 p-3 rounded-lg text-sm font-medium border ${isError ? 'bg-red-100 text-red-700 border-red-400' : 'bg-blue-100 text-blue-700 border-blue-400'}`}>
                            {message}
                        </div>
                    )}
                </Card>
            </div>
        );
    }
    
    return (
        <div className="min-h-screen flex items-center justify-center p-4 lg:p-8 bg-gray-50">
            <div className="w-full max-w-4xl">
                <div className="flex flex-col items-center justify-center space-y-8">
                    <div className="text-center space-y-2">
                        <div className="flex items-center justify-center gap-2 mb-4">
                            <Sparkles className="w-8 h-8 text-indigo-600" />
                            <h1 className="text-4xl lg:text-5xl font-bold tracking-tight text-gray-900">Picas y Famas</h1>
                        </div>
                        <p className="text-gray-600 text-lg">Descubre la combinación de 4 dígitos únicos</p>
                        <p className="text-sm text-indigo-600 font-semibold">
                            Jugador ID: <span className="font-mono">{playerId}</span>
                        </p>
                    </div>

                    {(gameId === null || gameWon) ? (
                        <>
                            <Card className="p-6 w-full max-w-md bg-gradient-to-br from-indigo-50 to-purple-50 border-2 border-indigo-200">
                                <h3 className="text-lg font-bold text-indigo-900 mb-4 text-center">🎮 ¿Cómo Jugar?</h3>
                                <div className="space-y-3 text-sm text-gray-700">
                                    <p className="font-semibold text-indigo-800">🎯 Objetivo:</p>
                                    <p>Adivina un número secreto de <strong>4 dígitos únicos</strong> (del 0 al 9, sin repetir).</p>
                                    
                                    <div className="bg-white rounded-lg p-3 space-y-2 border border-indigo-100">
                                        <p className="font-semibold text-green-700">✅ Fama:</p>
                                        <p>Un dígito está en la <strong>posición correcta</strong>.</p>
                                        <p className="text-xs text-gray-600 italic">Ejemplo: Si el secreto es 1234 y adivinas 1567, tienes 1 Fama (el 1).</p>
                                    </div>
                                    
                                    <div className="bg-white rounded-lg p-3 space-y-2 border border-indigo-100">
                                        <p className="font-semibold text-yellow-700">⚠️ Pica:</p>
                                        <p>Un dígito es correcto pero está en <strong>posición incorrecta</strong>.</p>
                                        <p className="text-xs text-gray-600 italic">Ejemplo: Si el secreto es 1234 y adivinas 5214, tienes 2 Picas (el 2 y el 4).</p>
                                    </div>
                                    
                                    <div className="bg-indigo-100 rounded-lg p-3 mt-3">
                                        <p className="font-semibold text-indigo-900">📊 Reglas:</p>
                                        <ul className="list-disc list-inside space-y-1 mt-2 text-xs">
                                            <li>Tienes <strong>10 intentos</strong> máximo</li>
                                            <li>Los 4 dígitos deben ser <strong>diferentes</strong></li>
                                            <li>Usa las pistas de Famas y Picas para acercarte</li>
                                        </ul>
                                    </div>
                                </div>
                            </Card>
                            
                            <Card className="p-8 lg:p-12 shadow-2xl border-2 border-green-500 w-full max-w-md">
                                <h3 className="text-center text-xl font-semibold mb-6">
                                    {gameWon ? "¡Juego Ganado! 🎉" : "Listo para Jugar"}
                                </h3>
                                <CustomButton 
                                    onClick={startGame}
                                    disabled={loading}
                                    className="w-full h-12 text-lg font-semibold bg-green-600 hover:bg-green-700"
                                    size="lg"
                                >
                                    {loading ? 'Iniciando...' : <span className="flex items-center justify-center"><Play className="w-5 h-5 mr-2" /> Empezar Nuevo Juego</span>}
                                </CustomButton>
                            </Card>
                        </>
                    ) : (
                        <Card className="p-8 lg:p-12 shadow-2xl border-2 border-indigo-500">
                            <div className="space-y-6">
                                <div className="text-center text-sm font-medium text-gray-600">
                                    <span>Intento actual: <span className="font-bold text-red-500">{attempts.length + 1}</span> / {MAX_ATTEMPTS}</span>
                                </div>
                                <div className="flex gap-3 lg:gap-4 justify-center">
                                    {currentGuess.map((digit, index) => (
                                        <input
                                            key={index}
                                            ref={inputRefs[index]}
                                            type="text"
                                            inputMode="numeric"
                                            maxLength={1}
                                            value={digit}
                                            onChange={(e) => handleInputChange(index, e.target.value)}
                                            onKeyDown={(e) => handleKeyDown(index, e)}
                                            disabled={loading}
                                            className={cn(
                                                "w-16 h-20 lg:w-20 lg:h-24 text-3xl lg:text-4xl font-bold text-center",
                                                "bg-gray-100 border-2 border-gray-300 rounded-xl",
                                                "focus:outline-none focus:ring-4 focus:ring-indigo-500/30 focus:border-indigo-500",
                                                "transition-all duration-200",
                                                "disabled:opacity-50 disabled:cursor-not-allowed",
                                                digit && "bg-indigo-50 border-indigo-500",
                                            )}
                                        />
                                    ))}
                                </div>

                                <CustomButton
                                    onClick={handleSubmit}
                                    disabled={currentGuess.some((d) => d === "") || loading}
                                    className="w-full h-12 text-lg font-semibold"
                                    size="lg"
                                >
                                    {loading ? 'Verificando...' : 'Verificar'}
                                </CustomButton>
                            </div>
                        </Card>
                    )}

                    {message && (
                        <Card className={`p-4 w-full max-w-md shadow-lg animate-in fade-in duration-300 ${isError ? 'bg-red-100 border-red-400' : 'bg-blue-100 border-blue-400'}`}>
                            <div className={`text-sm font-medium ${isError ? 'text-red-700' : 'text-blue-700'}`}>
                                {message}
                            </div>
                        </Card>
                    )}

                    {attempts.length > 0 && (
                        <div className="w-full max-w-md space-y-3">
                            <h3 className="text-sm font-semibold text-gray-600 uppercase tracking-wide">
                                Historial de Intentos
                            </h3>
                            <Card className="p-4 bg-blue-50 border border-blue-200">
                                <h4 className="font-semibold mb-2 text-sm text-gray-700">📚 Cómo interpretar los resultados:</h4>
                                <ul className="space-y-1 text-sm text-gray-600">
                                    <li className="flex gap-2">
                                        <span className="text-green-600 font-bold">Fama:</span>
                                        <span>Número correcto en la posición correcta.</span>
                                    </li>
                                    <li className="flex gap-2">
                                        <span className="text-yellow-600 font-bold">Pica:</span>
                                        <span>Número correcto pero en posición incorrecta.</span>
                                    </li>
                                </ul>
                            </Card>
                            <div className="space-y-2 max-h-64 overflow-y-auto pr-2">
                                {[...attempts].reverse().map((attempt, index) => (
                                    <Card
                                        key={attempts.length - index}
                                        className="p-4 flex items-center justify-between bg-gray-50 hover:bg-gray-100 transition-colors"
                                    >
                                        <div className="flex items-center gap-3">
                                            <span className="text-sm font-medium text-gray-500 w-6">#{attempts.length - index}</span>
                                            <div className="flex gap-2">
                                                {attempt.numbers.split('').map((num, i) => (
                                                    <div
                                                        key={i}
                                                        className={cn(
                                                            "w-8 h-8 flex items-center justify-center rounded-md font-mono font-bold border",
                                                            attempt.isCorrect ? "bg-green-100 border-green-500" : "bg-white border-gray-200"
                                                        )}
                                                    >
                                                        {num}
                                                    </div>
                                                ))}
                                            </div>
                                        </div>
                                        <div className="flex gap-4 text-sm font-semibold">
                                            <span className="text-green-600">
                                                {attempt.fama} {attempt.fama === 1 ? "Fama" : "Famas"}
                                            </span>
                                            <span className="text-yellow-600">
                                                {attempt.pica} {attempt.pica === 1 ? "Pica" : "Picas"}
                                            </span>
                                        </div>
                                    </Card>
                                ))}
                            </div>
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
}