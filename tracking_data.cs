using System;

namespace live2d_chat_server
{
    struct tracking_data{
        public int ParamAngleX = 0;
        public int ParamAngleY = 0;
        public int ParamAngleZ = 0;
        public int ParamEyeLOpen = 0;
        public int ParamEyeLSmile = 0;
        public int ParamEyeROpen = 0;
        public int ParamEyeRSmile = 0;
        public int ParamEyeBallX = 0;
        public int ParamEyeBallY = 0;
        public int? ParamEyeBallForm = null;
        public int ParamBrowLY = 0;
        public int ParamBrowRY = 0;
        public int ParamBrowLX = 0;
        public int ParamBrowRX = 0;
        public int ParamBrowLAngle = 0;
        public int ParamBrowRAngle = 0;
        public int ParamBrowLForm = 0;
        public int ParamBrowRForm = 0;
        public int ParamMouthForm = 0;
        public int ParamMouthOpenY = 0;
        public int ParamCheek = 0;
        public int ParamBodyAngleX = 0;
        public int ParamBodyAngleY = 0;
        public int ParamBodyAngleZ = 0;
        public int ParamBreath = 0;
        public int ParamHairFront = 0;
        public int ParamHairSide = 0;
        public int ParamHairBack = 0;

        public tracking_data(){}
    }

    enum tracking_data_type{
        ParamAngleX,
        ParamAngleY,
        ParamAngleZ,
        ParamEyeLOpen,
        ParamEyeLSmile,
        ParamEyeROpen,
        ParamEyeRSmile,
        ParamEyeBallX,
        ParamEyeBallY,
        ParamEyeBallForm,
        ParamBrowLY,
        ParamBrowRY,
        ParamBrowLX,
        ParamBrowRX,
        ParamBrowLAngle,
        ParamBrowRAngle,
        ParamBrowLForm,
        ParamBrowRForm,
        ParamMouthForm,
        ParamMouthOpenY,
        ParamCheek,
        ParamBodyAngleX,
        ParamBodyAngleY,
        ParamBodyAngleZ,
        ParamBreath,
        ParamHairFront,
        ParamHairSide,
        ParamHairBack,
    }
}
