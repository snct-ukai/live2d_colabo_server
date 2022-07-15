using System.ComponentModel;
using System.Collections.Generic;
using System;

namespace live2d_chat_server
{
    class tracking_data{
        private int[] data = new int[28];

        public tracking_data(byte[] buffer){
            for(int i = 0; i < buffer.Length; i++){
                if(i >= 28){
                    throw IndexOutOfRangeException();
                }
                byte[] tmp = buffer[(i * 4) .. (i * 4 + 4)];
                data[i] = BitConverter.ToInt32(tmp);
            }
        }

        public void set_data(byte[] buffer) => ref this.tracking_data(buffer);

        public int get_param(tracking_data_type param){
            return data[param];
        }
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
