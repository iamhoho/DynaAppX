<script setup>
import { ref, watch, onMounted } from 'vue'

const props = defineProps({
    required: {
        required: true
    },
    modelValue: {
    },
    lableName: {
    },
    attributeName: {
    }
}
)

const value = ref(props.modelValue)
const isJson = ref(false)
const tip = ref(`
<pre>
Please use standard JSON format and follow the specifications for the EntityReference type input.
Here is an example:
{
  "@odata.type": "Microsoft.Dynamics.CRM.account",
  "accountid": "CD67D78E-16BB-E611-9999-C4346BDC3C23"
}
</pre>
`)

const emit = defineEmits(['update:modelValue']);

watch(() => value.value, (newValue, oldValue) => {
    checkValue(newValue);
})

onMounted(() => {
    checkValue(value.value);
})

function checkValue(value) {
    let modelValue = value;
    if (!modelValue) {
        modelValue = null;
    }
    try {
        let Jvalue = JSON.parse(value);
        if (typeof Jvalue == 'object') {
            modelValue = Jvalue;
            isJson.value = !Array.isArray(Jvalue);
        }
        else {
            isJson.value = false;
        }
    } catch (error) {
        isJson.value = false;
    }
    emit('update:modelValue', modelValue);
}
</script>
<template>
    <div style="display: flex; flex-wrap: nowrap; flex-direction: row; margin: 1em; align-items: center;">
        <div class="controlLable">
            <p>{{ lableName }}</p>
            <p v-if="attributeName">{{ attributeName }}</p>
        </div>
        <el-input class="controlItemTextarea" v-model="value" placeholder="Please input" type="textarea" />
        <el-icon style="margin-left: 5px;" size="2em">
            <WarningFilled color="#F56C6C" v-if="(required && (modelValue == null || modelValue == '')) || !isJson" />
            <SuccessFilled color="#67C23A" v-else />
        </el-icon>
        <el-tooltip placement="top" effect="light" :content="tip" raw-content>
            <el-icon>
                <Warning />
            </el-icon>
        </el-tooltip>
    </div>
</template>