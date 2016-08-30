﻿using Kokoro.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Graphics.Prefabs
{
    class CubeFactory
    {
        static Mesh eObj;

        static CubeFactory()
        {
            Init();
        }

        private static void Init()
        {
            eObj = new Mesh();

            uint[] indices = new uint[] {30,1,1,102,2,1,291,3,1
,50,1,2,170,2,2,292,3,2
,35,1,3,107,2,3,293,3,3
,37,1,4,109,2,4,294,3,4
,94,4,5,274,5,5,295,3,5
,28,1,6,100,2,6,296,3,6
,51,6,6,123,7,6,297,8,6
,52,9,6,124,10,6,298,11,6
,40,12,6,112,13,6,299,14,6
,55,6,5,127,7,5,300,8,5
,56,9,5,128,10,5,301,11,5
,50,12,5,122,13,5,302,14,5
,59,6,4,131,7,4,303,8,4
,60,9,4,132,10,4,304,11,4
,49,12,4,121,13,4,305,14,4
,63,6,3,135,7,3,306,8,3
,64,9,3,136,10,3,307,11,3
,47,12,3,119,13,3,308,14,3
,80,15,2,218,16,2,309,8,2
,68,9,2,140,10,2,310,11,2
,78,17,2,210,18,2,311,14,2
,71,6,1,143,7,1,312,8,1
,72,9,1,144,10,1,313,11,1
,75,17,1,198,18,1,314,14,1
,72,9,1,197,19,1,315,20,1
,9,21,1,147,22,1,316,23,1
,27,24,1,99,25,1,317,26,1
,40,27,1,201,28,1,318,29,1
,39,30,1,200,31,1,319,32,1
,39,30,1,111,33,1,320,34,1
,77,15,1,203,35,1,321,36,1
,10,37,1,145,38,1,322,39,1
,73,40,1,193,41,1,323,42,1
,78,17,2,207,43,2,324,20,2
,36,24,2,208,44,2,325,23,2
,36,24,2,108,25,2,326,26,2
,79,45,2,211,46,2,327,29,2
,48,30,2,212,31,2,328,32,2
,18,21,2,188,47,2,329,34,2
,80,15,2,215,35,2,330,36,2
,69,40,2,216,48,2,331,39,2
,69,40,2,189,41,2,332,42,2
,81,17,3,219,43,3,333,20,3
,29,24,3,220,44,3,334,23,3
,5,49,3,167,50,3,335,26,3
,82,45,3,223,46,3,336,29,3
,41,30,3,224,31,3,337,32,3
,11,21,3,184,47,3,338,34,3
,83,15,3,227,35,3,339,36,3
,65,40,3,228,48,3,340,39,3
,24,51,3,183,52,3,341,42,3
,84,17,4,231,43,4,342,20,4
,31,24,4,232,44,4,343,23,4
,6,49,4,169,50,4,344,26,4
,85,45,4,235,46,4,345,29,4
,43,30,4,236,31,4,346,32,4
,13,21,4,180,47,4,347,34,4
,86,15,4,239,35,4,348,36,4
,61,40,4,240,48,4,349,39,4
,23,51,4,179,52,4,350,42,4
,87,17,5,243,43,5,351,20,5
,33,24,5,244,44,5,352,23,5
,7,49,5,170,50,5,353,26,5
,88,45,5,247,46,5,354,29,5
,45,30,5,248,31,5,355,32,5
,15,21,5,176,47,5,356,34,5
,89,15,5,251,35,5,357,36,5
,14,37,5,129,38,5,358,39,5
,22,51,5,175,52,5,359,42,5
,90,17,6,255,43,6,360,20,6
,41,24,6,256,44,6,361,23,6
,1,49,6,160,50,6,362,26,6
,91,45,6,259,46,6,363,29,6
,29,30,6,260,31,6,364,32,6
,11,21,6,172,47,6,365,34,6
,92,15,6,263,35,6,366,36,6
,53,40,6,264,48,6,367,39,6
,21,51,6,171,52,6,368,42,6
,51,6,6,269,53,6,369,54,6
,54,55,6,268,56,6,370,57,6
,10,58,6,148,59,6,371,60,6
,94,4,5,271,61,5,372,54,5
,22,51,5,130,62,5,373,57,5
,20,58,5,158,59,5,374,60,5
,95,4,4,275,61,4,375,54,4
,62,55,4,276,56,4,376,57,4
,19,58,4,157,59,4,377,60,4
,96,4,3,279,61,3,378,54,3
,66,55,3,280,56,3,379,57,3
,17,58,3,155,59,3,380,60,3
,97,4,2,283,61,2,381,54,2
,70,55,2,284,56,2,382,57,2
,20,58,2,122,59,2,383,60,2
,98,4,1,287,61,1,384,54,1
,74,55,1,288,56,1,385,57,1
,74,55,1,194,63,1,386,60,1
,98,4,1,290,5,1,287,61,1
,97,4,2,286,5,2,283,61,2
,96,4,3,282,5,3,279,61,3
,95,4,4,278,5,4,275,61,4
,34,64,5,271,61,5,106,65,5
,93,4,6,270,5,6,267,61,6
,92,15,6,266,16,6,263,35,6
,91,45,6,262,66,6,259,46,6
,90,17,6,258,18,6,255,43,6
,89,15,5,254,16,5,251,35,5
,88,45,5,250,66,5,247,46,5
,87,17,5,246,18,5,243,43,5
,86,15,4,242,16,4,239,35,4
,85,45,4,238,66,4,235,46,4
,84,17,4,234,18,4,231,43,4
,83,15,3,230,16,3,227,35,3
,82,45,3,226,66,3,223,46,3
,81,17,3,222,18,3,219,43,3
,49,67,2,215,35,2,121,68,2
,79,45,2,214,66,2,211,46,2
,70,55,2,207,43,2,190,63,2
,77,15,1,206,16,1,203,35,1
,76,45,1,202,66,1,199,46,1
,74,55,1,195,43,1,194,63,1
,26,51,1,144,10,1,146,62,1
,72,9,1,192,47,1,197,19,1
,75,17,1,196,44,1,198,18,1
,10,37,1,112,69,1,145,38,1
,1,70,1,159,71,1,160,72,1
,76,45,1,200,31,1,202,66,1
,28,73,1,205,74,1,100,75,1
,28,73,1,148,76,1,205,74,1
,77,15,1,204,48,1,206,16,1
,68,9,2,209,19,2,140,10,2
,18,21,2,156,22,2,188,47,2
,78,17,2,208,44,2,210,18,2
,47,27,2,213,28,2,119,69,2
,5,70,2,168,71,2,167,72,2
,48,30,2,120,33,2,212,31,2
,35,73,2,217,74,2,107,75,2
,17,37,2,141,38,2,155,76,2
,80,15,2,216,48,2,218,16,2
,64,9,3,221,19,3,136,10,3
,11,21,3,149,22,3,184,47,3
,29,24,3,101,25,3,220,44,3
,39,27,3,225,28,3,111,69,3
,1,70,3,161,71,3,159,72,3
,41,30,3,113,33,3,224,31,3
,27,73,3,229,74,3,99,75,3
,9,37,3,137,38,3,147,76,3
,65,40,3,185,41,3,228,48,3
,60,9,4,233,19,4,132,10,4
,13,21,4,151,22,4,180,47,4
,31,24,4,103,25,4,232,44,4
,42,27,4,237,28,4,114,69,4
,2,70,4,163,71,4,162,72,4
,43,30,4,115,33,4,236,31,4
,30,73,4,241,74,4,102,75,4
,12,37,4,133,38,4,150,76,4
,61,40,4,181,41,4,240,48,4
,56,9,5,245,19,5,128,10,5
,15,21,5,153,22,5,176,47,5
,33,24,5,105,25,5,244,44,5
,44,27,5,249,28,5,116,69,5
,3,70,5,165,71,5,164,72,5
,45,30,5,117,33,5,248,31,5
,32,73,5,253,74,5,104,75,5
,32,73,5,152,76,5,253,74,5
,57,40,5,177,41,5,252,48,5
,52,9,6,257,19,6,124,10,6
,11,21,6,113,22,6,172,47,6
,41,24,6,161,25,6,256,44,6
,48,27,6,261,28,6,120,69,6
,5,70,6,101,71,6,168,72,6
,29,30,6,149,33,6,260,31,6
,36,73,6,265,74,6,108,75,6
,18,37,6,125,38,6,156,76,6
,53,40,6,173,41,6,264,48,6
,16,77,6,123,7,6,118,78,6
,21,51,6,126,62,6,171,52,6
,54,55,6,174,63,6,268,56,6
,55,6,5,273,53,5,127,7,5
,55,6,5,175,52,5,273,53,5
,58,55,5,178,63,5,272,56,5
,59,6,4,277,53,4,131,7,4
,23,51,4,134,62,4,179,52,4
,62,55,4,182,63,4,276,56,4
,63,6,3,281,53,3,135,7,3
,24,51,3,138,62,3,183,52,3
,66,55,3,186,63,3,280,56,3
,67,6,2,285,53,2,139,7,2
,25,51,2,142,62,2,187,52,2
,70,55,2,190,63,2,284,56,2
,71,6,1,289,53,1,143,7,1
,26,51,1,146,62,1,191,52,1
,98,4,1,288,56,1,290,5,1
,290,5,1,386,60,1,30,1,1
,290,5,1,288,56,1,386,60,1
,288,56,1,74,55,1,386,60,1
,191,52,1,385,57,1,71,6,1
,191,52,1,146,62,1,385,57,1
,146,62,1,74,55,1,385,57,1
,143,7,1,384,54,1,14,77,1
,143,7,1,289,53,1,384,54,1
,289,53,1,98,4,1,384,54,1
,284,56,2,383,60,2,97,4,2
,284,56,2,190,63,2,383,60,2
,190,63,2,20,58,2,383,60,2
,187,52,2,382,57,2,67,6,2
,187,52,2,142,62,2,382,57,2
,142,62,2,70,55,2,382,57,2
,139,7,2,381,54,2,19,77,2
,139,7,2,285,53,2,381,54,2
,285,53,2,97,4,2,381,54,2
,280,56,3,380,60,3,96,4,3
,280,56,3,186,63,3,380,60,3
,186,63,3,17,58,3,380,60,3
,183,52,3,379,57,3,63,6,3
,183,52,3,138,62,3,379,57,3
,138,62,3,66,55,3,379,57,3
,135,7,3,378,54,3,13,77,3
,135,7,3,281,53,3,378,54,3
,281,53,3,96,4,3,378,54,3
,276,56,4,377,60,4,95,4,4
,276,56,4,182,63,4,377,60,4
,182,63,4,19,58,4,377,60,4
,179,52,4,376,57,4,59,6,4
,179,52,4,134,62,4,376,57,4
,134,62,4,62,55,4,376,57,4
,131,7,4,375,54,4,15,77,4
,131,7,4,277,53,4,375,54,4
,277,53,4,95,4,4,375,54,4
,272,56,5,374,60,5,94,4,5
,272,56,5,178,63,5,374,60,5
,178,63,5,20,58,5,374,60,5
,273,53,5,373,57,5,94,4,5
,273,53,5,175,52,5,373,57,5
,175,52,5,22,51,5,373,57,5
,127,7,5,372,54,5,16,77,5
,127,7,5,273,53,5,372,54,5
,273,53,5,94,4,5,372,54,5
,268,56,6,371,60,6,93,4,6
,268,56,6,174,63,6,371,60,6
,174,63,6,10,58,6,371,60,6
,171,52,6,370,57,6,51,6,6
,171,52,6,126,62,6,370,57,6
,126,62,6,54,55,6,370,57,6
,118,78,6,369,54,6,46,64,6
,118,78,6,123,7,6,369,54,6
,123,7,6,51,6,6,369,54,6
,264,48,6,368,42,6,92,15,6
,264,48,6,173,41,6,368,42,6
,173,41,6,21,51,6,368,42,6
,156,76,6,367,39,6,36,73,6
,156,76,6,125,38,6,367,39,6
,125,38,6,53,40,6,367,39,6
,108,75,6,366,36,6,8,79,6
,108,75,6,265,74,6,366,36,6
,265,74,6,92,15,6,366,36,6
,260,31,6,365,34,6,91,45,6
,260,31,6,149,33,6,365,34,6
,149,33,6,11,21,6,365,34,6
,168,72,6,364,32,6,48,27,6
,168,72,6,101,71,6,364,32,6
,101,71,6,29,30,6,364,32,6
,120,69,6,363,29,6,18,37,6
,120,69,6,261,28,6,363,29,6
,261,28,6,91,45,6,363,29,6
,256,44,6,362,26,6,90,17,6
,256,44,6,161,25,6,362,26,6
,161,25,6,1,49,6,362,26,6
,172,47,6,361,23,6,52,9,6
,172,47,6,113,22,6,361,23,6
,113,22,6,41,24,6,361,23,6
,124,10,6,360,20,6,21,51,6
,124,10,6,257,19,6,360,20,6
,257,19,6,90,17,6,360,20,6
,252,48,5,359,42,5,89,15,5
,252,48,5,177,41,5,359,42,5
,177,41,5,22,51,5,359,42,5
,253,74,5,358,39,5,89,15,5
,253,74,5,152,76,5,358,39,5
,152,76,5,14,37,5,358,39,5
,104,75,5,357,36,5,4,79,5
,104,75,5,253,74,5,357,36,5
,253,74,5,89,15,5,357,36,5
,248,31,5,356,34,5,88,45,5
,248,31,5,117,33,5,356,34,5
,117,33,5,15,21,5,356,34,5
,164,72,5,355,32,5,44,27,5
,164,72,5,165,71,5,355,32,5
,165,71,5,45,30,5,355,32,5
,116,69,5,354,29,5,14,37,5
,116,69,5,249,28,5,354,29,5
,249,28,5,88,45,5,354,29,5
,244,44,5,353,26,5,87,17,5
,244,44,5,105,25,5,353,26,5
,105,25,5,7,49,5,353,26,5
,176,47,5,352,23,5,56,9,5
,176,47,5,153,22,5,352,23,5
,153,22,5,33,24,5,352,23,5
,128,10,5,351,20,5,22,51,5
,128,10,5,245,19,5,351,20,5
,245,19,5,87,17,5,351,20,5
,240,48,4,350,42,4,86,15,4
,240,48,4,181,41,4,350,42,4
,181,41,4,23,51,4,350,42,4
,150,76,4,349,39,4,30,73,4
,150,76,4,133,38,4,349,39,4
,133,38,4,61,40,4,349,39,4
,102,75,4,348,36,4,3,79,4
,102,75,4,241,74,4,348,36,4
,241,74,4,86,15,4,348,36,4
,236,31,4,347,34,4,85,45,4
,236,31,4,115,33,4,347,34,4
,115,33,4,13,21,4,347,34,4
,162,72,4,346,32,4,42,27,4
,162,72,4,163,71,4,346,32,4
,163,71,4,43,30,4,346,32,4
,114,69,4,345,29,4,12,37,4
,114,69,4,237,28,4,345,29,4
,237,28,4,85,45,4,345,29,4
,232,44,4,344,26,4,84,17,4
,232,44,4,103,25,4,344,26,4
,103,25,4,6,49,4,344,26,4
,180,47,4,343,23,4,60,9,4
,180,47,4,151,22,4,343,23,4
,151,22,4,31,24,4,343,23,4
,132,10,4,342,20,4,23,51,4
,132,10,4,233,19,4,342,20,4
,233,19,4,84,17,4,342,20,4
,228,48,3,341,42,3,83,15,3
,228,48,3,185,41,3,341,42,3
,185,41,3,24,51,3,341,42,3
,147,76,3,340,39,3,27,73,3
,147,76,3,137,38,3,340,39,3
,137,38,3,65,40,3,340,39,3
,99,75,3,339,36,3,2,79,3
,99,75,3,229,74,3,339,36,3
,229,74,3,83,15,3,339,36,3
,224,31,3,338,34,3,82,45,3
,224,31,3,113,33,3,338,34,3
,113,33,3,11,21,3,338,34,3
,159,72,3,337,32,3,39,27,3
,159,72,3,161,71,3,337,32,3
,161,71,3,41,30,3,337,32,3
,111,69,3,336,29,3,9,37,3
,111,69,3,225,28,3,336,29,3
,225,28,3,82,45,3,336,29,3
,220,44,3,335,26,3,81,17,3
,220,44,3,101,25,3,335,26,3
,101,25,3,5,49,3,335,26,3
,184,47,3,334,23,3,64,9,3
,184,47,3,149,22,3,334,23,3
,149,22,3,29,24,3,334,23,3
,136,10,3,333,20,3,24,51,3
,136,10,3,221,19,3,333,20,3
,221,19,3,81,17,3,333,20,3
,218,16,2,332,42,2,67,6,2
,218,16,2,216,48,2,332,42,2
,216,48,2,69,40,2,332,42,2
,155,76,2,331,39,2,35,73,2
,155,76,2,141,38,2,331,39,2
,141,38,2,69,40,2,331,39,2
,107,75,2,330,36,2,6,79,2
,107,75,2,217,74,2,330,36,2
,217,74,2,80,15,2,330,36,2
,212,31,2,329,34,2,79,45,2
,212,31,2,120,33,2,329,34,2
,120,33,2,18,21,2,329,34,2
,167,72,2,328,32,2,47,27,2
,167,72,2,168,71,2,328,32,2
,168,71,2,48,30,2,328,32,2
,119,69,2,327,29,2,17,37,2
,119,69,2,213,28,2,327,29,2
,213,28,2,79,45,2,327,29,2
,210,18,2,326,26,2,38,12,2
,210,18,2,208,44,2,326,26,2
,208,44,2,36,24,2,326,26,2
,188,47,2,325,23,2,68,9,2
,188,47,2,156,22,2,325,23,2
,156,22,2,36,24,2,325,23,2
,140,10,2,324,20,2,25,51,2
,140,10,2,209,19,2,324,20,2
,209,19,2,78,17,2,324,20,2
,206,16,1,323,42,1,71,6,1
,206,16,1,204,48,1,323,42,1
,204,48,1,73,40,1,323,42,1
,205,74,1,322,39,1,77,15,1
,205,74,1,148,76,1,322,39,1
,148,76,1,10,37,1,322,39,1
,100,75,1,321,36,1,4,79,1
,100,75,1,205,74,1,321,36,1
,205,74,1,77,15,1,321,36,1
,202,66,1,320,34,1,72,9,1
,202,66,1,200,31,1,320,34,1
,200,31,1,39,30,1,320,34,1
,160,72,1,319,32,1,40,27,1
,160,72,1,159,71,1,319,32,1
,159,71,1,39,30,1,319,32,1
,145,38,1,318,29,1,73,40,1
,145,38,1,112,69,1,318,29,1
,112,69,1,40,27,1,318,29,1
,198,18,1,317,26,1,42,12,1
,198,18,1,196,44,1,317,26,1
,196,44,1,27,24,1,317,26,1
,197,19,1,316,23,1,75,17,1
,197,19,1,192,47,1,316,23,1
,192,47,1,9,21,1,316,23,1
,146,62,1,315,20,1,74,55,1
,146,62,1,144,10,1,315,20,1
,144,10,1,72,9,1,315,20,1
,194,63,1,314,14,1,12,58,1
,194,63,1,195,43,1,314,14,1
,195,43,1,75,17,1,314,14,1
,199,46,1,313,11,1,73,40,1
,199,46,1,202,66,1,313,11,1
,202,66,1,72,9,1,313,11,1
,203,35,1,312,8,1,32,67,1
,203,35,1,206,16,1,312,8,1
,206,16,1,71,6,1,312,8,1
,190,63,2,311,14,2,20,58,2
,190,63,2,207,43,2,311,14,2
,207,43,2,78,17,2,311,14,2
,211,46,2,310,11,2,69,40,2
,211,46,2,214,66,2,310,11,2
,214,66,2,68,9,2,310,11,2
,121,68,2,309,8,2,19,77,2
,121,68,2,215,35,2,309,8,2
,215,35,2,80,15,2,309,8,2
,219,43,3,308,14,3,66,55,3
,219,43,3,222,18,3,308,14,3
,222,18,3,47,12,3,308,14,3
,223,46,3,307,11,3,65,40,3
,223,46,3,226,66,3,307,11,3
,226,66,3,64,9,3,307,11,3
,227,35,3,306,8,3,43,67,3
,227,35,3,230,16,3,306,8,3
,230,16,3,63,6,3,306,8,3
,231,43,4,305,14,4,62,55,4
,231,43,4,234,18,4,305,14,4
,234,18,4,49,12,4,305,14,4
,235,46,4,304,11,4,61,40,4
,235,46,4,238,66,4,304,11,4
,238,66,4,60,9,4,304,11,4
,239,35,4,303,8,4,45,67,4
,239,35,4,242,16,4,303,8,4
,242,16,4,59,6,4,303,8,4
,243,43,5,302,14,5,58,55,5
,243,43,5,246,18,5,302,14,5
,246,18,5,50,12,5,302,14,5
,247,46,5,301,11,5,57,40,5
,247,46,5,250,66,5,301,11,5
,250,66,5,56,9,5,301,11,5
,251,35,5,300,8,5,46,67,5
,251,35,5,254,16,5,300,8,5
,254,16,5,55,6,5,300,8,5
,255,43,6,299,14,6,54,55,6
,255,43,6,258,18,6,299,14,6
,258,18,6,40,12,6,299,14,6
,259,46,6,298,11,6,53,40,6
,259,46,6,262,66,6,298,11,6
,262,66,6,52,9,6,298,11,6
,263,35,6,297,8,6,34,67,6
,263,35,6,266,16,6,297,8,6
,266,16,6,51,6,6,297,8,6
,267,61,6,296,3,6,46,64,6
,267,61,6,270,5,6,296,3,6
,270,5,6,28,1,6,296,3,6
,106,65,5,295,3,5,8,80,5
,106,65,5,271,61,5,295,3,5
,271,61,5,94,4,5,295,3,5
,275,61,4,294,3,4,33,64,4
,275,61,4,278,5,4,294,3,4
,278,5,4,37,1,4,294,3,4
,279,61,3,293,3,3,31,64,3
,279,61,3,282,5,3,293,3,3
,282,5,3,35,1,3,293,3,3
,283,61,2,292,3,2,37,64,2
,283,61,2,286,5,2,292,3,2
,286,5,2,50,1,2,292,3,2
,287,61,1,291,3,1,44,64,1
,287,61,1,290,5,1,291,3,1
,290,5,1,30,1,1,291,3,1
,386,60,1,150,59,1,30,1,1
,386,60,1,194,63,1,150,59,1
,194,63,1,12,58,1,150,59,1
,385,57,1,289,53,1,71,6,1
,385,57,1,288,56,1,289,53,1
,288,56,1,98,4,1,289,53,1
,384,54,1,116,78,1,14,77,1
,384,54,1,287,61,1,116,78,1
,287,61,1,44,64,1,116,78,1
,383,60,2,286,5,2,97,4,2
,383,60,2,122,59,2,286,5,2
,122,59,2,50,1,2,286,5,2
,382,57,2,285,53,2,67,6,2
,382,57,2,284,56,2,285,53,2
,284,56,2,97,4,2,285,53,2
,381,54,2,157,78,2,19,77,2
,381,54,2,283,61,2,157,78,2
,283,61,2,37,64,2,157,78,2
,380,60,3,282,5,3,96,4,3
,380,60,3,155,59,3,282,5,3
,155,59,3,35,1,3,282,5,3
,379,57,3,281,53,3,63,6,3
,379,57,3,280,56,3,281,53,3
,280,56,3,96,4,3,281,53,3
,378,54,3,151,78,3,13,77,3
,378,54,3,279,61,3,151,78,3
,279,61,3,31,64,3,151,78,3
,377,60,4,278,5,4,95,4,4
,377,60,4,157,59,4,278,5,4
,157,59,4,37,1,4,278,5,4
,376,57,4,277,53,4,59,6,4
,376,57,4,276,56,4,277,53,4
,276,56,4,95,4,4,277,53,4
,375,54,4,153,78,4,15,77,4
,375,54,4,275,61,4,153,78,4
,275,61,4,33,64,4,153,78,4
,374,60,5,274,5,5,94,4,5
,374,60,5,158,59,5,274,5,5
,158,59,5,38,1,5,274,5,5
,373,57,5,272,56,5,94,4,5
,373,57,5,130,62,5,272,56,5
,130,62,5,58,55,5,272,56,5
,372,54,5,154,78,5,16,77,5
,372,54,5,271,61,5,154,78,5
,271,61,5,34,64,5,154,78,5
,371,60,6,270,5,6,93,4,6
,371,60,6,148,59,6,270,5,6
,148,59,6,28,1,6,270,5,6
,370,57,6,269,53,6,51,6,6
,370,57,6,268,56,6,269,53,6
,268,56,6,93,4,6,269,53,6
,369,54,6,267,61,6,46,64,6
,369,54,6,269,53,6,267,61,6
,269,53,6,93,4,6,267,61,6
,368,42,6,266,16,6,92,15,6
,368,42,6,171,52,6,266,16,6
,171,52,6,51,6,6,266,16,6
,367,39,6,265,74,6,36,73,6
,367,39,6,264,48,6,265,74,6
,264,48,6,92,15,6,265,74,6
,366,36,6,106,81,6,8,79,6
,366,36,6,263,35,6,106,81,6
,263,35,6,34,67,6,106,81,6
,365,34,6,262,66,6,91,45,6
,365,34,6,172,47,6,262,66,6
,172,47,6,52,9,6,262,66,6
,364,32,6,261,28,6,48,27,6
,364,32,6,260,31,6,261,28,6
,260,31,6,91,45,6,261,28,6
,363,29,6,125,38,6,18,37,6
,363,29,6,259,46,6,125,38,6
,259,46,6,53,40,6,125,38,6
,362,26,6,258,18,6,90,17,6
,362,26,6,160,50,6,258,18,6
,160,50,6,40,12,6,258,18,6
,361,23,6,257,19,6,52,9,6
,361,23,6,256,44,6,257,19,6
,256,44,6,90,17,6,257,19,6
,360,20,6,126,62,6,21,51,6
,360,20,6,255,43,6,126,62,6
,255,43,6,54,55,6,126,62,6
,359,42,5,254,16,5,89,15,5
,359,42,5,175,52,5,254,16,5
,175,52,5,55,6,5,254,16,5
,358,39,5,252,48,5,89,15,5
,358,39,5,129,38,5,252,48,5
,129,38,5,57,40,5,252,48,5
,357,36,5,166,81,5,4,79,5
,357,36,5,251,35,5,166,81,5
,251,35,5,46,67,5,166,81,5
,356,34,5,250,66,5,88,45,5
,356,34,5,176,47,5,250,66,5
,176,47,5,56,9,5,250,66,5
,355,32,5,249,28,5,44,27,5
,355,32,5,248,31,5,249,28,5
,248,31,5,88,45,5,249,28,5
,354,29,5,129,38,5,14,37,5
,354,29,5,247,46,5,129,38,5
,247,46,5,57,40,5,129,38,5
,353,26,5,246,18,5,87,17,5
,353,26,5,170,50,5,246,18,5
,170,50,5,50,12,5,246,18,5
,352,23,5,245,19,5,56,9,5
,352,23,5,244,44,5,245,19,5
,244,44,5,87,17,5,245,19,5
,351,20,5,130,62,5,22,51,5
,351,20,5,243,43,5,130,62,5
,243,43,5,58,55,5,130,62,5
,350,42,4,242,16,4,86,15,4
,350,42,4,179,52,4,242,16,4
,179,52,4,59,6,4,242,16,4
,349,39,4,241,74,4,30,73,4
,349,39,4,240,48,4,241,74,4
,240,48,4,86,15,4,241,74,4
,348,36,4,165,81,4,3,79,4
,348,36,4,239,35,4,165,81,4
,239,35,4,45,67,4,165,81,4
,347,34,4,238,66,4,85,45,4
,347,34,4,180,47,4,238,66,4
,180,47,4,60,9,4,238,66,4
,346,32,4,237,28,4,42,27,4
,346,32,4,236,31,4,237,28,4
,236,31,4,85,45,4,237,28,4
,345,29,4,133,38,4,12,37,4
,345,29,4,235,46,4,133,38,4
,235,46,4,61,40,4,133,38,4
,344,26,4,234,18,4,84,17,4
,344,26,4,169,50,4,234,18,4
,169,50,4,49,12,4,234,18,4
,343,23,4,233,19,4,60,9,4
,343,23,4,232,44,4,233,19,4
,232,44,4,84,17,4,233,19,4
,342,20,4,134,62,4,23,51,4
,342,20,4,231,43,4,134,62,4
,231,43,4,62,55,4,134,62,4
,341,42,3,230,16,3,83,15,3
,341,42,3,183,52,3,230,16,3
,183,52,3,63,6,3,230,16,3
,340,39,3,229,74,3,27,73,3
,340,39,3,228,48,3,229,74,3
,228,48,3,83,15,3,229,74,3
,339,36,3,163,81,3,2,79,3
,339,36,3,227,35,3,163,81,3
,227,35,3,43,67,3,163,81,3
,338,34,3,226,66,3,82,45,3
,338,34,3,184,47,3,226,66,3
,184,47,3,64,9,3,226,66,3
,337,32,3,225,28,3,39,27,3
,337,32,3,224,31,3,225,28,3
,224,31,3,82,45,3,225,28,3
,336,29,3,137,38,3,9,37,3
,336,29,3,223,46,3,137,38,3
,223,46,3,65,40,3,137,38,3
,335,26,3,222,18,3,81,17,3
,335,26,3,167,50,3,222,18,3
,167,50,3,47,12,3,222,18,3
,334,23,3,221,19,3,64,9,3
,334,23,3,220,44,3,221,19,3
,220,44,3,81,17,3,221,19,3
,333,20,3,138,62,3,24,51,3
,333,20,3,219,43,3,138,62,3
,219,43,3,66,55,3,138,62,3
,332,42,2,187,52,2,67,6,2
,332,42,2,189,41,2,187,52,2
,189,41,2,25,51,2,187,52,2
,331,39,2,217,74,2,35,73,2
,331,39,2,216,48,2,217,74,2
,216,48,2,80,15,2,217,74,2
,330,36,2,169,81,2,6,79,2
,330,36,2,215,35,2,169,81,2
,215,35,2,49,67,2,169,81,2
,329,34,2,214,66,2,79,45,2
,329,34,2,188,47,2,214,66,2
,188,47,2,68,9,2,214,66,2
,328,32,2,213,28,2,47,27,2
,328,32,2,212,31,2,213,28,2
,212,31,2,79,45,2,213,28,2
,327,29,2,141,38,2,17,37,2
,327,29,2,211,46,2,141,38,2
,211,46,2,69,40,2,141,38,2
,326,26,2,110,50,2,38,12,2
,326,26,2,108,25,2,110,50,2
,108,25,2,8,49,2,110,50,2
,325,23,2,209,19,2,68,9,2
,325,23,2,208,44,2,209,19,2
,208,44,2,78,17,2,209,19,2
,324,20,2,142,62,2,25,51,2
,324,20,2,207,43,2,142,62,2
,207,43,2,70,55,2,142,62,2
,323,42,1,191,52,1,71,6,1
,323,42,1,193,41,1,191,52,1
,193,41,1,26,51,1,191,52,1
,322,39,1,204,48,1,77,15,1
,322,39,1,145,38,1,204,48,1
,145,38,1,73,40,1,204,48,1
,321,36,1,104,81,1,4,79,1
,321,36,1,203,35,1,104,81,1
,203,35,1,32,67,1,104,81,1
,320,34,1,192,47,1,72,9,1
,320,34,1,111,33,1,192,47,1
,111,33,1,9,21,1,192,47,1
,319,32,1,201,28,1,40,27,1
,319,32,1,200,31,1,201,28,1
,200,31,1,76,45,1,201,28,1
,318,29,1,199,46,1,73,40,1
,318,29,1,201,28,1,199,46,1
,201,28,1,76,45,1,199,46,1
,317,26,1,162,50,1,42,12,1
,317,26,1,99,25,1,162,50,1
,99,25,1,2,49,1,162,50,1
,316,23,1,196,44,1,75,17,1
,316,23,1,147,22,1,196,44,1
,147,22,1,27,24,1,196,44,1
,315,20,1,195,43,1,74,55,1
,315,20,1,197,19,1,195,43,1
,197,19,1,75,17,1,195,43,1
,314,14,1,114,13,1,12,58,1
,314,14,1,198,18,1,114,13,1
,198,18,1,42,12,1,114,13,1
,313,11,1,193,41,1,73,40,1
,313,11,1,144,10,1,193,41,1
,144,10,1,26,51,1,193,41,1
,312,8,1,152,68,1,32,67,1
,312,8,1,143,7,1,152,68,1
,143,7,1,14,77,1,152,68,1
,311,14,2,158,13,2,20,58,2
,311,14,2,210,18,2,158,13,2
,210,18,2,38,12,2,158,13,2
,310,11,2,189,41,2,69,40,2
,310,11,2,140,10,2,189,41,2
,140,10,2,25,51,2,189,41,2
,309,8,2,139,7,2,19,77,2
,309,8,2,218,16,2,139,7,2
,218,16,2,67,6,2,139,7,2
,308,14,3,186,63,3,66,55,3
,308,14,3,119,13,3,186,63,3
,119,13,3,17,58,3,186,63,3
,307,11,3,185,41,3,65,40,3
,307,11,3,136,10,3,185,41,3
,136,10,3,24,51,3,185,41,3
,306,8,3,115,68,3,43,67,3
,306,8,3,135,7,3,115,68,3
,135,7,3,13,77,3,115,68,3
,305,14,4,182,63,4,62,55,4
,305,14,4,121,13,4,182,63,4
,121,13,4,19,58,4,182,63,4
,304,11,4,181,41,4,61,40,4
,304,11,4,132,10,4,181,41,4
,132,10,4,23,51,4,181,41,4
,303,8,4,117,68,4,45,67,4
,303,8,4,131,7,4,117,68,4
,131,7,4,15,77,4,117,68,4
,302,14,5,178,63,5,58,55,5
,302,14,5,122,13,5,178,63,5
,122,13,5,20,58,5,178,63,5
,301,11,5,177,41,5,57,40,5
,301,11,5,128,10,5,177,41,5
,128,10,5,22,51,5,177,41,5
,300,8,5,118,68,5,46,67,5
,300,8,5,127,7,5,118,68,5
,127,7,5,16,77,5,118,68,5
,299,14,6,174,63,6,54,55,6
,299,14,6,112,13,6,174,63,6
,112,13,6,10,58,6,174,63,6
,298,11,6,173,41,6,53,40,6
,298,11,6,124,10,6,173,41,6
,124,10,6,21,51,6,173,41,6
,297,8,6,154,68,6,34,67,6
,297,8,6,123,7,6,154,68,6
,123,7,6,16,77,6,154,68,6
,296,3,6,166,65,6,46,64,6
,296,3,6,100,2,6,166,65,6
,100,2,6,4,80,6,166,65,6
,295,3,5,110,2,5,8,80,5
,295,3,5,274,5,5,110,2,5
,274,5,5,38,1,5,110,2,5
,294,3,4,105,65,4,33,64,4
,294,3,4,109,2,4,105,65,4
,109,2,4,7,80,4,105,65,4
,293,3,3,103,65,3,31,64,3
,293,3,3,107,2,3,103,65,3
,107,2,3,6,80,3,103,65,3
,292,3,2,109,65,2,37,64,2
,292,3,2,170,2,2,109,65,2
,170,2,2,7,80,2,109,65,2
,291,3,1,164,65,1,44,64,1
,291,3,1,102,2,1,164,65,1
,102,2,1,3,80,1,164,65,1

            };

            double[] vers = new double[]{
1.000000,-1.000000,-1.000000
,1.000000,-1.000000,1.000000
,-1.000000,-1.000000,1.000000
,-1.000000,-1.000000,-1.000000
,1.000000,1.000000,-0.999999
,0.999999,1.000000,1.000001
,-1.000000,1.000000,1.000000
,-1.000000,1.000000,-1.000000
,1.000000,-1.000000,0.000000
,0.000000,-1.000000,-1.000000
,1.000000,0.000000,-1.000000
,0.000000,-1.000000,1.000000
,1.000000,0.000000,1.000000
,-1.000000,-1.000000,-0.000000
,-1.000000,0.000000,1.000000
,-1.000000,0.000000,-1.000000
,1.000000,1.000000,0.000001
,0.000000,1.000000,-1.000000
,-0.000001,1.000000,1.000000
,-1.000000,1.000000,-0.000000
,0.000000,0.000000,-1.000000
,-1.000000,0.000000,-0.000000
,-0.000000,0.000000,1.000000
,1.000000,0.000000,0.000000
,-0.000000,1.000000,0.000000
,0.000000,-1.000000,-0.000000
,1.000000,-1.000000,0.500000
,-0.500000,-1.000000,-1.000000
,1.000000,0.500000,-1.000000
,-0.500000,-1.000000,1.000000
,1.000000,0.500000,1.000000
,-1.000000,-1.000000,-0.500000
,-1.000000,0.500000,1.000000
,-1.000000,0.500000,-1.000000
,1.000000,1.000000,0.500001
,-0.500000,1.000000,-1.000000
,-0.500000,1.000000,1.000000
,-1.000000,1.000000,-0.500000
,1.000000,-1.000000,-0.500000
,0.500000,-1.000000,-1.000000
,1.000000,-0.500000,-1.000000
,0.500000,-1.000000,1.000000
,1.000000,-0.500000,1.000000
,-1.000000,-1.000000,0.500000
,-1.000000,-0.500000,1.000000
,-1.000000,-0.500000,-1.000000
,1.000000,1.000000,-0.499999
,0.500000,1.000000,-1.000000
,0.499999,1.000000,1.000000
,-1.000000,1.000000,0.500000
,-0.500000,0.000000,-1.000000
,0.500000,0.000000,-1.000000
,0.000000,0.500000,-1.000000
,0.000000,-0.500000,-1.000000
,-1.000000,0.000000,-0.500000
,-1.000000,0.000000,0.500000
,-1.000000,-0.500000,-0.000000
,-1.000000,0.500000,-0.000000
,-0.500000,0.000000,1.000000
,0.500000,0.000000,1.000000
,-0.000000,-0.500000,1.000000
,-0.000000,0.500000,1.000000
,1.000000,0.000000,0.500000
,1.000000,0.000000,-0.500000
,1.000000,-0.500000,0.000000
,1.000000,0.500000,0.000000
,-0.000000,1.000000,0.500000
,0.000000,1.000000,-0.500000
,0.500000,1.000000,0.000000
,-0.500000,1.000000,0.000000
,-0.500000,-1.000000,-0.000000
,0.500000,-1.000000,-0.000000
,0.000000,-1.000000,-0.500000
,0.000000,-1.000000,0.500000
,0.500000,-1.000000,0.500000
,0.500000,-1.000000,-0.500000
,-0.500000,-1.000000,-0.500000
,-0.500000,1.000000,-0.500000
,0.500000,1.000000,-0.500000
,0.500000,1.000000,0.500000
,1.000000,0.500000,-0.500000
,1.000000,-0.500000,-0.500000
,1.000000,-0.500000,0.500000
,0.500000,0.500000,1.000000
,0.500000,-0.500000,1.000000
,-0.500000,-0.500000,1.000000
,-1.000000,0.500000,0.500000
,-1.000000,-0.500000,0.500000
,-1.000000,-0.500000,-0.500000
,0.500000,-0.500000,-1.000000
,0.500000,0.500000,-1.000000
,-0.500000,0.500000,-1.000000
,-0.500000,-0.500000,-1.000000
,-1.000000,0.500000,-0.500000
,-0.500000,0.500000,1.000000
,1.000000,0.500000,0.500000
,-0.500000,1.000000,0.500000
,-0.500000,-1.000000,0.500000
,1.000000,-1.000000,0.750000
,-0.750000,-1.000000,-1.000000
,1.000000,0.750000,-1.000000
,-0.750000,-1.000000,1.000000
,0.999999,0.750000,1.000000
,-1.000000,-1.000000,-0.750000
,-1.000000,0.750000,1.000000
,-1.000000,0.750000,-1.000000
,1.000000,1.000000,0.750001
,-0.750000,1.000000,-1.000000
,-0.750000,1.000000,1.000000
,-1.000000,1.000000,-0.750000
,1.000000,-1.000000,-0.250000
,0.250000,-1.000000,-1.000000
,1.000000,-0.250000,-1.000000
,0.250000,-1.000000,1.000000
,1.000000,-0.250000,1.000000
,-1.000000,-1.000000,0.250000
,-1.000000,-0.250000,1.000000
,-1.000000,-0.250000,-1.000000
,1.000000,1.000000,-0.249999
,0.250000,1.000000,-1.000000
,0.249999,1.000000,1.000000
,-1.000000,1.000000,0.250000
,-0.750000,0.000000,-1.000000
,0.250000,0.000000,-1.000000
,0.000000,0.750000,-1.000000
,0.000000,-0.250000,-1.000000
,-1.000000,0.000000,-0.750000
,-1.000000,0.000000,0.250000
,-1.000000,-0.750000,-0.000000
,-1.000000,0.250000,-0.000000
,-0.750000,0.000000,1.000000
,0.250000,0.000000,1.000000
,-0.000000,-0.750000,1.000000
,-0.000000,0.250000,1.000000
,1.000000,0.000000,0.750000
,1.000000,0.000000,-0.250000
,1.000000,-0.750000,0.000000
,1.000000,0.250000,0.000000
,-0.000000,1.000000,0.750000
,-0.000000,1.000000,-0.250000
,0.750000,1.000000,0.000000
,-0.250000,1.000000,0.000000
,-0.750000,-1.000000,-0.000000
,0.250000,-1.000000,-0.000000
,0.000000,-1.000000,-0.750000
,0.000000,-1.000000,0.250000
,1.000000,-1.000000,0.250000
,-0.250000,-1.000000,-1.000000
,1.000000,0.250000,-1.000000
,-0.250000,-1.000000,1.000000
,1.000000,0.250000,1.000000
,-1.000000,-1.000000,-0.250000
,-1.000000,0.250000,1.000000
,-1.000000,0.250000,-1.000000
,1.000000,1.000000,0.250001
,-0.250000,1.000000,-1.000000
,-0.250001,1.000000,1.000000
,-1.000000,1.000000,-0.250000
,1.000000,-1.000000,-0.750000
,0.750000,-1.000000,-1.000000
,1.000000,-0.750000,-1.000000
,0.750000,-1.000000,1.000000
,1.000000,-0.750000,1.000000
,-1.000000,-1.000000,0.750000
,-1.000000,-0.750000,1.000000
,-1.000000,-0.750000,-1.000000
,1.000000,1.000000,-0.749999
,0.750000,1.000000,-1.000000
,0.749999,1.000000,1.000000
,-1.000000,1.000000,0.750000
,-0.250000,0.000000,-1.000000
,0.750000,0.000000,-1.000000
,0.000000,0.250000,-1.000000
,0.000000,-0.750000,-1.000000
,-1.000000,0.000000,-0.250000
,-1.000000,0.000000,0.750000
,-1.000000,-0.250000,-0.000000
,-1.000000,0.750000,-0.000000
,-0.250000,0.000000,1.000000
,0.750000,0.000000,1.000000
,-0.000000,-0.250000,1.000000
,-0.000000,0.750000,1.000000
,1.000000,0.000000,0.250000
,1.000000,0.000000,-0.750000
,1.000000,-0.250000,0.000000
,1.000000,0.750000,0.000000
,-0.000000,1.000000,0.250000
,0.000000,1.000000,-0.750000
,0.250000,1.000000,0.000000
,-0.750000,1.000000,-0.000000
,-0.250000,-1.000000,-0.000000
,0.750000,-1.000000,0.000000
,0.000000,-1.000000,-0.250000
,0.000000,-1.000000,0.750000
,0.250000,-1.000000,0.500000
,0.750000,-1.000000,0.500000
,0.500000,-1.000000,0.250000
,0.500000,-1.000000,0.750000
,0.250000,-1.000000,-0.500000
,0.750000,-1.000000,-0.500000
,0.500000,-1.000000,-0.750000
,0.500000,-1.000000,-0.250000
,-0.750000,-1.000000,-0.500000
,-0.250000,-1.000000,-0.500000
,-0.500000,-1.000000,-0.750000
,-0.500000,-1.000000,-0.250000
,-0.500000,1.000000,-0.250000
,-0.500000,1.000000,-0.750000
,-0.250000,1.000000,-0.500000
,-0.750000,1.000000,-0.500000
,0.500000,1.000000,-0.250000
,0.500000,1.000000,-0.750000
,0.750000,1.000000,-0.500000
,0.250000,1.000000,-0.500000
,0.500000,1.000000,0.750000
,0.500000,1.000000,0.250000
,0.750000,1.000000,0.500000
,0.250000,1.000000,0.500000
,1.000000,0.500000,-0.250000
,1.000000,0.500000,-0.750000
,1.000000,0.250000,-0.500000
,1.000000,0.750000,-0.500000
,1.000000,-0.500000,-0.250000
,1.000000,-0.500000,-0.750000
,1.000000,-0.750000,-0.500000
,1.000000,-0.250000,-0.500000
,1.000000,-0.500000,0.750000
,1.000000,-0.500000,0.250000
,1.000000,-0.750000,0.500000
,1.000000,-0.250000,0.500000
,0.250000,0.500000,1.000000
,0.750000,0.500000,1.000000
,0.500000,0.250000,1.000000
,0.499999,0.750000,1.000000
,0.250000,-0.500000,1.000000
,0.750000,-0.500000,1.000000
,0.500000,-0.750000,1.000000
,0.500000,-0.250000,1.000000
,-0.750000,-0.500000,1.000000
,-0.250000,-0.500000,1.000000
,-0.500000,-0.750000,1.000000
,-0.500000,-0.250000,1.000000
,-1.000000,0.500000,0.250000
,-1.000000,0.500000,0.750000
,-1.000000,0.250000,0.500000
,-1.000000,0.750000,0.500000
,-1.000000,-0.500000,0.250000
,-1.000000,-0.500000,0.750000
,-1.000000,-0.750000,0.500000
,-1.000000,-0.250000,0.500000
,-1.000000,-0.500000,-0.750000
,-1.000000,-0.500000,-0.250000
,-1.000000,-0.750000,-0.500000
,-1.000000,-0.250000,-0.500000
,0.250000,-0.500000,-1.000000
,0.750000,-0.500000,-1.000000
,0.500000,-0.250000,-1.000000
,0.500000,-0.750000,-1.000000
,0.250000,0.500000,-1.000000
,0.750000,0.500000,-1.000000
,0.500000,0.750000,-1.000000
,0.500000,0.250000,-1.000000
,-0.750000,0.500000,-1.000000
,-0.250000,0.500000,-1.000000
,-0.500000,0.750000,-1.000000
,-0.500000,0.250000,-1.000000
,-0.750000,-0.500000,-1.000000
,-0.250000,-0.500000,-1.000000
,-0.500000,-0.250000,-1.000000
,-0.500000,-0.750000,-1.000000
,-1.000000,0.500000,-0.750000
,-1.000000,0.500000,-0.250000
,-1.000000,0.250000,-0.500000
,-1.000000,0.750000,-0.500000
,-0.750000,0.500000,1.000000
,-0.250000,0.500000,1.000000
,-0.500000,0.250000,1.000000
,-0.500000,0.750000,1.000000
,1.000000,0.500000,0.750000
,1.000000,0.500000,0.250000
,1.000000,0.250000,0.500000
,1.000000,0.750000,0.500000
,-0.500000,1.000000,0.750000
,-0.500000,1.000000,0.250000
,-0.250000,1.000000,0.500000
,-0.750000,1.000000,0.500000
,-0.750000,-1.000000,0.500000
,-0.250000,-1.000000,0.500000
,-0.500000,-1.000000,0.250000
,-0.500000,-1.000000,0.750000
,-0.750000,-1.000000,0.750000
,-0.750000,1.000000,0.750000
,1.000000,0.750000,0.750000
,-0.750000,0.750000,1.000000
,-1.000000,0.750000,-0.750000
,-0.750000,-0.750000,-1.000000
,-0.750000,0.250000,-1.000000
,0.250000,0.250000,-1.000000
,0.250000,-0.750000,-1.000000
,-1.000000,-0.250000,-0.750000
,-1.000000,-0.250000,0.250000
,-1.000000,0.750000,0.250000
,-0.750000,-0.250000,1.000000
,0.250000,-0.250000,1.000000
,0.249999,0.750000,1.000000
,1.000000,-0.250000,0.750000
,1.000000,-0.250000,-0.250000
,1.000000,0.750000,-0.250000
,0.250000,1.000000,0.750000
,0.250000,1.000000,-0.250000
,-0.750000,1.000000,-0.250000
,-0.750000,-1.000000,-0.250000
,0.250000,-1.000000,-0.250000
,0.250000,-1.000000,0.750000
,0.250000,-1.000000,0.250000
,0.750000,-1.000000,0.250000
,0.750000,-1.000000,0.750000
,0.250000,-1.000000,-0.750000
,0.750000,-1.000000,-0.750000
,0.750000,-1.000000,-0.250000
,-0.750000,-1.000000,-0.750000
,-0.250000,-1.000000,-0.750000
,-0.250000,-1.000000,-0.250000
,-0.250000,1.000000,-0.250000
,-0.250000,1.000000,-0.750000
,-0.750000,1.000000,-0.750000
,0.750000,1.000000,-0.250000
,0.750000,1.000000,-0.750000
,0.250000,1.000000,-0.750000
,0.750000,1.000000,0.750000
,0.750000,1.000000,0.250000
,0.250000,1.000000,0.250000
,1.000000,0.250000,-0.250000
,1.000000,0.250000,-0.750000
,1.000000,0.750000,-0.750000
,1.000000,-0.750000,-0.250000
,1.000000,-0.750000,-0.750000
,1.000000,-0.250000,-0.750000
,1.000000,-0.750000,0.750000
,1.000000,-0.750000,0.250000
,1.000000,-0.250000,0.250000
,0.250000,0.250000,1.000000
,0.750000,0.250000,1.000000
,0.749999,0.750000,1.000000
,0.250000,-0.750000,1.000000
,0.750000,-0.750000,1.000000
,0.750000,-0.250000,1.000000
,-0.750000,-0.750000,1.000000
,-0.250000,-0.750000,1.000000
,-0.250000,-0.250000,1.000000
,-1.000000,0.250000,0.250000
,-1.000000,0.250000,0.750000
,-1.000000,0.750000,0.750000
,-1.000000,-0.750000,0.250000
,-1.000000,-0.750000,0.750000
,-1.000000,-0.250000,0.750000
,-1.000000,-0.750000,-0.750000
,-1.000000,-0.750000,-0.250000
,-1.000000,-0.250000,-0.250000
,0.250000,-0.250000,-1.000000
,0.750000,-0.250000,-1.000000
,0.750000,-0.750000,-1.000000
,0.250000,0.750000,-1.000000
,0.750000,0.750000,-1.000000
,0.750000,0.250000,-1.000000
,-0.750000,0.750000,-1.000000
,-0.250000,0.750000,-1.000000
,-0.250000,0.250000,-1.000000
,-0.750000,-0.250000,-1.000000
,-0.250000,-0.250000,-1.000000
,-0.250000,-0.750000,-1.000000
,-1.000000,0.250000,-0.750000
,-1.000000,0.250000,-0.250000
,-1.000000,0.750000,-0.250000
,-0.750000,0.250000,1.000000
,-0.250000,0.250000,1.000000
,-0.250000,0.750000,1.000000
,1.000000,0.250000,0.750000
,1.000000,0.250000,0.250000
,1.000000,0.750000,0.250000
,-0.250000,1.000000,0.750000
,-0.250000,1.000000,0.250000
,-0.750000,1.000000,0.250000
,-0.750000,-1.000000,0.250000
,-0.250000,-1.000000,0.250000
,-0.250000,-1.000000,0.750000
            };

            double[] uvs = new double[]
            {1.000000,0.750000
,1.000000,0.875000
,0.875000,0.875000
,0.750000,0.750000
,0.875000,0.750000
,0.500000,0.750000
,0.500000,0.875000
,0.375000,0.875000
,0.500000,0.250000
,0.500000,0.375000
,0.375000,0.375000
,1.000000,0.250000
,1.000000,0.375000
,0.875000,0.375000
,0.250000,0.750000
,0.375000,0.750000
,0.750000,0.250000
,0.875000,0.250000
,0.625000,0.250000
,0.625000,0.375000
,0.500000,0.000000
,0.625000,0.000000
,0.625000,0.125000
,0.750000,0.000000
,0.875000,0.000000
,0.875000,0.125000
,0.000000,0.250000
,0.125000,0.250000
,0.125000,0.375000
,0.250000,0.000000
,0.250000,0.125000
,0.125000,0.125000
,0.375000,0.000000
,0.375000,0.125000
,0.250000,0.875000
,0.125000,0.875000
,0.000000,0.500000
,0.125000,0.500000
,0.125000,0.625000
,0.250000,0.500000
,0.375000,0.500000
,0.375000,0.625000
,0.750000,0.375000
,0.750000,0.125000
,0.250000,0.250000
,0.250000,0.375000
,0.500000,0.125000
,0.250000,0.625000
,1.000000,0.000000
,1.000000,0.125000
,0.500000,0.500000
,0.500000,0.625000
,0.625000,0.750000
,0.625000,0.875000
,0.750000,0.500000
,0.750000,0.625000
,0.625000,0.625000
,1.000000,0.500000
,1.000000,0.625000
,0.875000,0.625000
,0.750000,0.875000
,0.625000,0.500000
,0.875000,0.500000
,0.750000,1.000000
,0.875000,1.000000
,0.375000,0.250000
,0.250000,1.000000
,0.375000,1.000000
,0.000000,0.375000
,0.000000,0.000000
,0.125000,0.000000
,0.000000,0.125000
,0.000000,0.750000
,0.125000,0.750000
,0.000000,0.875000
,0.000000,0.625000
,0.500000,1.000000
,0.625000,1.000000
,0.000000,1.000000
,1.000000,1.000000
,0.125000,1.000000
            };

            float[] norm_p = new float[]
            {
0.0f,-1.0f,0.0f
,0.0f,1.0f,0.0f
,1.0f,0.0f,0.0f
,-0.0f,0.0f,1.0f
,-1.0f,-0.0f,-0.0f
,0.0f,0.0f,-1.0f
            };

            List<float> norms = new List<float>();
            List<float> uvs_l = new List<float>();
            List<float> verts = new List<float>();
            List<uint> inds = new List<uint>();

            for (int i = 0; i < indices.Length; i += 3)
            {
                uint v_ind = indices[i] - 1;
                uint u_ind = indices[i + 1] - 1;
                uint n_ind = indices[i + 2] - 1;

                inds.Add((uint)i / 3);

                uvs_l.Add((float)uvs[u_ind * 2]);
                uvs_l.Add((float)uvs[u_ind * 2 + 1]);

                norms.Add(norm_p[n_ind * 3]);
                norms.Add(norm_p[n_ind * 3 + 1]);
                norms.Add(norm_p[n_ind * 3 + 2]);

                verts.Add((float)vers[v_ind * 3]);
                verts.Add((float)vers[v_ind * 3 + 1]);
                verts.Add((float)vers[v_ind * 3 + 2]);
            }

            eObj.SetIndices(0, inds.ToArray(), false);
            eObj.SetUVs(0, uvs_l.ToArray(), false);
            eObj.SetNormals(0, norms.ToArray(), false);
            eObj.SetVertices(0, verts.ToArray(), false);
        }

        public static Mesh Create()
        {
            return new Mesh(eObj, true);    //Lock the buffers from changes
        }
    }
}