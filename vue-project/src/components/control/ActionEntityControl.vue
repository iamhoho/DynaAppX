<script setup>
import { ref, watch, onMounted } from 'vue'

const props = defineProps({
    required: {
        required: true
    },
    modelValue: {
    },
    attName: {
    }
}
)

const value = ref(props.modelValue)
const isJson = ref(false)
const tip = ref(`
      <pre>
Please use standard JSON format and follow the specifications for the Entity type input.
Here is an example:
{
  "@odata.type": "Microsoft.Dynamics.CRM.account",
  "accountid": "CD67D78E-16BB-E611-9999-C4346BDC3C22",
  "name": "hoho",
  "accountnumber": "123456"
}
      </pre>
`)

const emit = defineEmits(['update:modelValue']);

watch(() => value.value, (newValue, oldValue) => {
    checkValue(newValue);
    emit('update:modelValue', newValue);
})

onMounted(() => {
    checkValue(value.value);
})

function checkValue(value) {
    try {
        let Jvalue = JSON.parse(value);
        if (typeof Jvalue == 'object') {
            isJson.value = !Array.isArray(Jvalue);
        }
        else {
            isJson.value = false;
        }
    } catch (error) {
        isJson.value = false;
    }
    emit('update:modelValue', value);
}
</script>
<template>
    <div style="display: flex; flex-wrap: nowrap; flex-direction: row; margin: 1em;">
        <p>{{ attName }}</p>
        <el-input v-model="value" placeholder="Please input" type="textarea" />
        <el-tooltip placement="top" effect="light" :content="tip" raw-content>
            <el-icon>
                <Warning />
            </el-icon>
        </el-tooltip>
        <el-icon size="2em"
            :color="(required && (modelValue == null || modelValue == '')) || !isJson ? '#F56C6C' : '#67C23A'">
            <SuccessFilled />
        </el-icon>
    </div>
</template>