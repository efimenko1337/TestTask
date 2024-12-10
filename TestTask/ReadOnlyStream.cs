using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TestTask
{
    public class ReadOnlyStream : IReadOnlyStream
    {
        private readonly Stream _localStream;
        private readonly UTF8Encoding strictUtf8 = new UTF8Encoding(false, true);

        /// <summary>
        /// Конструктор класса. 
        /// Т.к. происходит прямая работа с файлом, необходимо 
        /// обеспечить ГАРАНТИРОВАННОЕ закрытие файла после окончания работы с таковым!
        /// </summary>
        /// <param name="fileFullPath">Полный путь до файла для чтения</param>
        public ReadOnlyStream(string fileFullPath)
        {
            _localStream = new FileStream(fileFullPath, FileMode.Open);

            IsEof = false;
        }

        ~ReadOnlyStream()
        {
            Close();
        }

        public void Close() =>
            _localStream.Close();

        /// <summary>
        /// Флаг окончания файла.
        /// </summary>
        public bool IsEof
        {
            get;
            private set;
        }

        /// <summary>
        /// Ф-ция чтения следующего символа из потока.
        /// Если произведена попытка прочитать символ после достижения конца файла, метод 
        /// должен бросать соответствующее исключение
        /// </summary>
        /// <returns>Считанный символ.</returns>
        public char ReadNextChar()
        {
            if (IsEof)
                throw new EndOfStreamException("The stream has run out of characters.");

            var buffer = new List<byte>();
            int bytesRead = 0;


            while (true)
            {
                int res = _localStream.ReadByte();
                if (res == -1)
                    throw new EndOfStreamException("The stream has run out of characters.");
                else
                    buffer.Add((byte)res);

                bytesRead++;

                try
                {
                    string decodedString = strictUtf8.GetString(buffer.ToArray(), 0, bytesRead);
                    if (decodedString.Length == 1)
                    {
                        IsEof = _localStream.Position >= _localStream.Length;
                        return decodedString[0];
                    }
                }
                catch (DecoderFallbackException)
                {
                    if (bytesRead >= 4)
                    {
                        throw new InvalidOperationException("Invalid UTF-8 byte sequence.");
                    }
                }

            }
        }

        /// <summary>
        /// Сбрасывает текущую позицию потока на начало.
        /// </summary>
        public void ResetPositionToStart()
        {
            if (_localStream == null)
            {
                IsEof = true;
                return;
            }

            _localStream.Position = 0;
            IsEof = false;
        }
    }
}